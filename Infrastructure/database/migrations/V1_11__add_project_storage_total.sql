ALTER TABLE public.project
ADD COLUMN storage_total_bytes BIGINT,
ADD COLUMN storage_local_bytes BIGINT,
ADD COLUMN storage_last_calculated TIMESTAMP;