document.addEventListener("DOMContentLoaded", function () {

    const addButton = document.getElementById("add-question");
    const container = document.getElementById("questions-container");

    if (!addButton || !container) return;

    addButton.addEventListener("click", function () {

        const index = container.querySelectorAll(".question-row").length;

        const wrapper = document.createElement("div");
        wrapper.className = "question-row border rounded p-3 mb-2";

        wrapper.innerHTML = `
            <div class="mb-2">
                <label class="form-label">Question text</label>
                <input class="form-control"
                       name="Questions[${index}].Text" />
            </div>

            <div class="row">
                <div class="col-md-4 mb-2">
                    <label class="form-label">Answer type</label>
                    <select class="form-select"
                            name="Questions[${index}].Type">
                        <option value="Scale10">1 - 10</option>
                        <option value="YesNo">Yes / No</option>
                        <option value="Text">Text</option>
                        <option value="Dropdown">Dropdown</option>
                    </select>
                </div>

                <div class="col-md-3 mb-2 d-flex align-items-center">
                    <!-- MVC checkbox pattern -->
                    <input type="hidden"
                           name="Questions[${index}].Required"
                           value="false" />

                    <input type="checkbox"
                           class="form-check-input me-2"
                           name="Questions[${index}].Required"
                           value="true"
                           checked />

                    <label class="form-check-label">Required</label>
                </div>
            </div>
        `;

        container.appendChild(wrapper);
    });
});
