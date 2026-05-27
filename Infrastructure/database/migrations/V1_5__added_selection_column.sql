CREATE TABLE selection_session
(
    id         serial PRIMARY KEY,
    project_id integer      NOT NULL,
    name       varchar(255) NOT NULL,
    created_at timestamp    NOT NULL DEFAULT now(),

    FOREIGN KEY (project_id) REFERENCES project (id) ON DELETE CASCADE
);

CREATE TABLE selection_session_image
(
    selection_session_id integer NOT NULL,
    image_id             integer NOT NULL,

    PRIMARY KEY (selection_session_id, image_id),

    FOREIGN KEY (selection_session_id) REFERENCES selection_session (id) ON DELETE CASCADE,
    FOREIGN KEY (image_id) REFERENCES image (id) ON DELETE CASCADE
);
