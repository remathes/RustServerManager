-- Rust Player Stats MySQL Schema

CREATE TABLE servers (
    id INT PRIMARY KEY AUTO_INCREMENT,
    identity VARCHAR(128) NOT NULL,
    name VARCHAR(255),
    region VARCHAR(64),
    created DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE players (
    steam_id BIGINT PRIMARY KEY,
    name VARCHAR(128) NOT NULL,
    first_seen DATETIME,
    last_seen DATETIME,
    total_played TIME
);

CREATE TABLE player_stats (
    id INT PRIMARY KEY AUTO_INCREMENT,
    steam_id BIGINT NOT NULL,
    server_id INT NOT NULL,
    kills INT DEFAULT 0,
    deaths INT DEFAULT 0,
    headshots INT DEFAULT 0,
    gathered INT DEFAULT 0,
    looted_items INT DEFAULT 0,
    structures_built INT DEFAULT 0,
    favorite_weapon VARCHAR(64),
    favorite_resource VARCHAR(64),
    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (steam_id) REFERENCES players(steam_id),
    FOREIGN KEY (server_id) REFERENCES servers(id)
);

CREATE TABLE player_items (
    id INT PRIMARY KEY AUTO_INCREMENT,
    steam_id BIGINT NOT NULL,
    server_id INT NOT NULL,
    shortname VARCHAR(64) NOT NULL,
    quantity INT DEFAULT 0,
    last_seen DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (steam_id) REFERENCES players(steam_id),
    FOREIGN KEY (server_id) REFERENCES servers(id)
);
CREATE TABLE instances (
    id INT PRIMARY KEY AUTO_INCREMENT,
    server_id INT NOT NULL,
    identity VARCHAR(20) NOT NULL,
    server_hostname VARCHAR(50) NOT NULL,
    server_url VARCHAR(100) NOT NULL,
    description VARCHAR(100) NOT NULL,
    server_ip VARCHAR(15) DEFAULT '127.0.0.1',
    server_port INT DEFAULT 28015,
    rcon_ip VARCHAR(15) DEFAULT '127.0.0.1',
    rcon_port INT DEFAULT 28016,
    rcon_password VARCHAR(20) NOT NULL,
    rcon_web INT DEFAULT 0,
    max_players INT DEFAULT 100,
    server_tickrate INT DEFAULT 1,
    server_save_interval INT DEFAULT 300,
    use_custom_map BOOL DEFAULT 0,
    seed INT DEFAULT 0,
    world_size INT DEFAULT 1000,
    map_name VARCHAR(30) DEFAULT '',
    server_level_url VARCHAR(100) DEFAULT '',
    steam_cmd_path VARCHAR(100) DEFAULT '',



-- Optional Indexes
CREATE INDEX idx_player_stats_steam_server ON player_stats(steam_id, server_id);
CREATE INDEX idx_player_items_steam_server ON player_items(steam_id, server_id);
