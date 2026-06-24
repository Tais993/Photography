CREATE TABLE image_metadata
(
    image_id       integer      NOT NULL,
    metadata_key   varchar(255) NOT NULL,
    metadata_value VARCHAR(255),

    PRIMARY KEY (image_id, metadata_key),

    FOREIGN KEY (image_id) REFERENCES image (id) ON DELETE CASCADE,
    FOREIGN KEY (metadata_key) REFERENCES metadata (metadata_key) ON DELETE CASCADE
);