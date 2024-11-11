-- 17. 퀘스트 테이블 생성
CREATE TABLE quests(
	quest_id INT AUTO_INCREMENT PRIMARY KEY,
	title VARCHAR(100) NOT NULL,
	description TEXT,
	reward_exp INT DEFAULT 0,
	reward_item_id INT,
	FOREIGN KEY (reward_item_id) REFERENCES items(item_id)
);

-- 18.퀘스트데이터 입력
INSERT INTO quests (title ,description ,reward_exp, reward_item_id) VALUES
('초보자 퀘스트' , '첫 번째 퀘스트를 완료 하세요' , 100, 3),
('용사의 검', '전설의 검을 찾아오세요', 500, 1);

-- 19. 플레이어 퀘스트 진행 상황 테이블
CREATE TABLE player_quests(
	player_id INT,
	quest_id INT,
	status ENUM('시작','진행중','완료') DEFAULT '시작',
	started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	completed_at TIMESTAMP NULL,
	PRIMARY KEY (player_id, quest_id),
	FOREIGN KEY (player_id) REFERENCES players(player_id),
	FOREIGN KEY (quest_id) REFERENCES quests(quest_id)
); 

-- 20. 플레이어에게 퀘스트 할당
INSERT INTO player_quests (player_id, quest_id) VALUES
(1,1),	-- 1번 플레이어에게 초보자 퀘스트 할당
(2,2);	-- 2번 플레이어에게 용사의 검 퀘스트 할당

-- 21. 진행중인 퀘스트 조회
SELECT p.username , q.title, pq.status
FROM players p
JOIN player_quests pq ON p.player_id = pq.player_id
JOIN quests q ON pq.quest_id = q.quest_id
WHERE pq.status != '완료';

-- 22. 퀘스트 완료 처리
UPDATE player_quests
SET STATUS = '완료', completed_at = CURRENT_TIMESTAMP
WHERE player_id = 1 AND quest_id = 1;

-- 실습
-- 새로운 퀘스트 추가
INSERT INTO quests (title ,description  ,reward_exp, reward_item_id) VALUES
('중급자 퀘스트', '중급자 완료 하세요' , 600, 2)
-- 특정 플레이어의 모든 퀘스트 상태 조회
SELECT q.title , pq.status FROM player_quests pq
JOIN quests q ON pq.quest_id = q.quest_id WHERE pq.player_id = 1

-- 가장 많은 경험치를 주는 퀘스트 출력
SELECT title, reward_exp FROM quests ORDER BY reward_exp DESC LIMIT 1;