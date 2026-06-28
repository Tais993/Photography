document.addEventListener("input", function (event) {
    if (event.target.id === "create-project-name" || event.target.id === "create-project-date") {
        updateCreateProjectFolderPreview();
    }
});

document.addEventListener("shown.bs.modal", function (event) {
    if (event.target.id === "create-project-modal") {
        updateCreateProjectFolderPreview();
    }
});

function updateCreateProjectFolderPreview() {
    const preview = document.getElementById("create-project-folder-preview");
    const nameInput = document.getElementById("create-project-name");
    const dateInput = document.getElementById("create-project-date");

    if (!preview || !nameInput || !dateInput) {
        return;
    }

    const projectName = nameInput.value.trim();
    const projectDate = dateInput.value;

    if (!projectName || !projectDate) {
        preview.textContent = "Enter a project name to preview the folder name.";
        return;
    }

    preview.textContent = `${projectDate}-${projectName}`;
}