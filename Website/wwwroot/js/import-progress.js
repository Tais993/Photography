class ImportProgress {
    constructor() {
        this.card = document.getElementById("import-progress-card");
        this.icon = document.getElementById("import-progress-icon");
        this.title = document.getElementById("import-progress-title");
        this.text = document.getElementById("import-progress-text");
        this.bar = document.getElementById("import-progress-bar");
        this.count = document.getElementById("import-progress-count");
        this.percentage = document.getElementById("import-progress-percentage");

        this.importId = null;
        this.pollTimeout = null;
    }

    show() {
        if (this.card === null) {
            return;
        }

        this.card.classList.remove("d-none");
    }

    hide() {
        if (this.card === null) {
            return;
        }

        this.card.classList.add("d-none");
    }

    start(totalFiles, importId = null) {
        this.show();
        this.setImportingState();

        this.importId = importId;

        this.update({
            percentage: 0,
            totalFiles: totalFiles ?? 0,
            filesImported: 0
        });

        if (importId !== null && importId !== "") {
            this.startPolling();
        }
    }

    update(progress) {
        this.show();

        const percentage = progress.percentage ?? 0;
        const totalFiles = progress.totalFiles ?? 0;
        const filesImported = progress.filesImported ?? 0;
        const currentFile = progress.currentFile ?? null;

        if (this.bar !== null) {
            this.bar.style.width = `${percentage}%`;
            this.bar.setAttribute("aria-valuenow", percentage);
        }

        if (this.text !== null) {
            if (currentFile !== null && currentFile !== "") {
                this.text.textContent = `Importing ${currentFile}`;
            } else {
                this.text.textContent = `Importing image ${filesImported} out of ${totalFiles}`;
            }
        }

        if (this.count !== null) {
            this.count.textContent = `${filesImported} / ${totalFiles} images`;
        }

        if (this.percentage !== null) {
            this.percentage.textContent = `${percentage}%`;
        }
    }

    complete(progress) {
        this.stopPolling();
        this.show();

        const totalFiles = progress?.totalFiles ?? progress?.filesImported ?? 0;
        const filesImported = progress?.filesImported ?? totalFiles;

        this.update({
            percentage: 100,
            totalFiles: totalFiles,
            filesImported: filesImported
        });

        if (this.title !== null) {
            this.title.textContent = "Import completed";
        }

        if (this.text !== null) {
            this.text.textContent = "All images have been imported.";
        }

        if (this.icon !== null) {
            this.icon.classList.remove("bg-blue-lt");
            this.icon.classList.remove("bg-red-lt");
            this.icon.classList.add("bg-green-lt");
            this.icon.innerHTML = '<i class="ti ti-check"></i>';
        }

        window.setTimeout(() => {
            this.hide();
        }, 3000);
    }

    fail(message) {
        this.stopPolling();
        this.show();

        if (this.title !== null) {
            this.title.textContent = "Import failed";
        }

        if (this.text !== null) {
            this.text.textContent = message || "Something went wrong while importing images.";
        }

        if (this.icon !== null) {
            this.icon.classList.remove("bg-blue-lt");
            this.icon.classList.remove("bg-green-lt");
            this.icon.classList.add("bg-red-lt");
            this.icon.innerHTML = '<i class="ti ti-alert-triangle"></i>';
        }

        if (this.card !== null) {
            this.card.classList.add("border-danger");
        }
    }

    setImportingState() {
        if (this.title !== null) {
            this.title.textContent = "Importing images";
        }

        if (this.text !== null) {
            this.text.textContent = "Starting import...";
        }

        if (this.icon !== null) {
            this.icon.classList.remove("bg-green-lt");
            this.icon.classList.remove("bg-red-lt");
            this.icon.classList.add("bg-blue-lt");
            this.icon.innerHTML = '<i class="ti ti-upload"></i>';
        }

        if (this.card !== null) {
            this.card.classList.remove("border-danger");
        }
    }

    startPolling() {
        this.stopPolling();
        this.poll();
    }

    stopPolling() {
        if (this.pollTimeout !== null) {
            window.clearTimeout(this.pollTimeout);
            this.pollTimeout = null;
        }
    }

    async poll() {
        if (this.importId === null || this.importId === "") {
            return;
        }

        try {
            const response = await fetch(`?handler=ImportProgress&importId=${encodeURIComponent(this.importId)}`, {
                credentials: "same-origin"
            });

            if (!response.ok) {
                this.pollTimeout = window.setTimeout(() => this.poll(), 1000);
                return;
            }

            const progress = await response.json();

            this.update(progress);

            if (progress.hasFailed) {
                this.fail(progress.errorMessage);
                return;
            }

            if (progress.isCompleted) {
                this.complete(progress);
                return;
            }

            this.pollTimeout = window.setTimeout(() => this.poll(), 1000);
        } catch {
            this.pollTimeout = window.setTimeout(() => this.poll(), 1000);
        }
    }
}

window.importProgress = new ImportProgress();

window.startImportProgress = function startImportProgress(totalFiles, importId = null) {
    window.importProgress.start(totalFiles, importId);
};

window.updateImportProgress = function updateImportProgress(progress) {
    window.importProgress.update(progress);
};

window.completeImportProgress = function completeImportProgress(progress) {
    window.importProgress.complete(progress);
};

window.failImportProgress = function failImportProgress(message) {
    window.importProgress.fail(message);
};

window.showImportStarted = function showImportStarted() {
    const modalElement = document.getElementById("create-project-modal");

    if (modalElement !== null) {
        const modal = bootstrap.Modal.getInstance(modalElement);

        if (modal !== null) {
            modal.hide();
        }
    }

    startImportProgress(0);
};