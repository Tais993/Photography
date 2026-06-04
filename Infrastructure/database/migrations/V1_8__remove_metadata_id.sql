ALTER TABLE public.project_metadata
    DROP CONSTRAINT IF EXISTS project_metadata_pkey;

ALTER TABLE public.project_metadata
    DROP CONSTRAINT IF EXISTS project_metadata_metadata_id_fkey;

ALTER TABLE public.project_metadata
    DROP COLUMN IF EXISTS metadata_id;

ALTER TABLE public.metadata
    DROP CONSTRAINT IF EXISTS metadata_pkey;

ALTER TABLE public.metadata
    DROP COLUMN IF EXISTS id;

ALTER TABLE public.project_metadata
    ADD COLUMN metadata_key varchar(255) NOT NULL;

ALTER TABLE public.project_metadata
    ADD CONSTRAINT project_metadata_pkey
        PRIMARY KEY (project_id, metadata_key);

ALTER TABLE public.project_metadata
    ADD CONSTRAINT project_metadata_metadata_key_fkey
        FOREIGN KEY (metadata_key)
            REFERENCES public.metadata(metadata_key)
            ON UPDATE CASCADE
            ON DELETE CASCADE;