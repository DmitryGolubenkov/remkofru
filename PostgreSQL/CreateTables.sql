USE remkof_db;

CREATE TABLE IF NOT EXISTS users (
    user_id SERIAL PRIMARY KEY,
    username varchar(30) NOT NULL,
    email varchar(60) NOT NULL,
    password_hash varchar(512) NOT NULL,
    password_salt varchar(60) NOT NULL,
    is_activated boolean NOT NULL
);

CREATE TABLE IF NOT EXISTS prices (
	price_id SERIAL PRIMARY KEY,
	view_priority INT NOT NULL,
	serv_name varchar(150) NOT NULL,
	price varchar(50) NOT NULL
);

--CREATE TABLE item_for_sale(
--    id SERIAL PRIMARY KEY,
--    product_name varchar(100) NOT NULL,
--    product_description varchar(100) NOT NULL,
--    price real NOT NULL,
--    product_image bytea NOT NULL
--);