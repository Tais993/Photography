CREATE TABLE project
(
    id          serial PRIMARY KEY,
    name        VARCHAR(255) NOT NULL,
    location    VARCHAR(255) NOT NULL,
    date        DATE
);

CREATE TABLE metadata
(
    id            serial PRIMARY KEY,
    metadata_key  VARCHAR(255) NOT NULL,
    metadata_type VARCHAR(255) NOT NULL,
    display_name  VARCHAR(255) NOT NULL,
    description   VARCHAR(255)
);

CREATE TABLE image
(
    id         serial PRIMARY KEY,
    project_id int REFERENCES project (id),
    file_name  VARCHAR(255) NOT NULL,
    file_type  VARCHAR(255) NOT NULL,
    file_path  VARCHAR(260) NOT NULL
);

CREATE TABLE project_metadata
(
    project_id     integer NOT NULL,
    metadata_id    integer NOT NULL,
    metadata_value VARCHAR(255),

    PRIMARY KEY (project_id, metadata_id),

    FOREIGN KEY (project_id) REFERENCES project (id) ON DELETE CASCADE,
    FOREIGN KEY (metadata_id) REFERENCES metadata (id) ON DELETE CASCADE
);

