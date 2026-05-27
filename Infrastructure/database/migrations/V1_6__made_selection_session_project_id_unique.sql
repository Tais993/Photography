ALTER TABLE public.selection_session
ADD CONSTRAINT selection_session_project_id_unique UNIQUE (project_id);