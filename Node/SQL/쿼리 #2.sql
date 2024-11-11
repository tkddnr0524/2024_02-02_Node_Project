-- 10. 아이템 테이블 생성

CREATE TABLE items (
	item_id INT AUTO_INCREMENT PRIMARY KEY,
	name VARCHAR(100) NOT NULL,
	description TEXT,
	value INT DEFAULT 0
	);
	
-- 11. 아이템 데이터 삽입
INSERT INTO items(name , description , value) VALUES
('검' , '기본 무기', 10),
('방패' , '기본 방어구', 15),
('몰락' , '체력을 회복', 5);

-- 12. 아이템 조회
SELECT * FROM items;

-- 13. 플레이어 인벤토리 테이블 생성
CREATE TABLE inventories(
	inventory_id INT AUTO_INCREMENT PRIMARY KEY,
	player_id INT,
	item_id INT,
	quantity INT DEFAULT 1,
	FOREIGN KEY(player_id) REFERENCES players(player_id),
	FOREIGN KEY(item_id) REFERENCES items(item_id)
	)
	
-- 14. 인벤토리에 아이템 추가
INSERT INTO inventories (player_id, item_id, quantity) VALUES
(1,1,1),	-- 1번플레이어에 검 1개
(1,3,5), -- 1번플레이어에 물약 5개
(2,2,1); -- 2번플레이어에 방패 1

-- 15. 플레이어의 인벤토리 조회
SELECT p.username, i.name, inv.quantity
FROM players p
JOIN inventories inv ON p.player_id = inv.player_id
JOIN items i ON inv.item_id = i.item_id;

-- 16. 특정 플레이어의 인벤토리 가치 계산
SELECT p.username, SUM(i.value * inv.quantity) AS total_value
FROM players p
JOIN inventories inv ON p.player_id = inv.player_id
JOIN items i ON inv.item_id = i.item_id
GROUP BY p.player_id;

-- 실습
-- 1. 새로운 아이템 추가
INSERT INTO items(name , description, value) VALUES
('용검' , '용 기본 무기', 20)
-- 2.	특정 플레이어의 인벤토리에 새 아이템 추가
INSERT INTO inventories (player_id, item_id, quantity) VALUES
(2,4,2)
-- 3. 가장 가치 있는 아이템 찾기
SELECT name , value FROM items ORDER BY value DESC LIMIT 1;
