--USE remkof_db;

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
	service_name varchar(150) NOT NULL,
	price varchar(50) NOT NULL,
	view_priority INT NOT NULL
);

CREATE TABLE IF NOT EXISTS options (
    key_name varchar(150) PRIMARY KEY NOT NULL,
    key_value varchar(150)
);

--CREATE TABLE item_for_sale(
--    id SERIAL PRIMARY KEY,
--    product_name varchar(100) NOT NULL,
--    product_description varchar(100) NOT NULL,
--    price real NOT NULL,
--    product_image bytea NOT NULL
--);