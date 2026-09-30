class UI {
    static $ = (selector) => document.querySelector(selector);
    static $$ = (selector) => document.querySelectorAll(selector);

    /**
     * Obtiene todos los valores de un formulario y los mete en un objeto JSON
     * Busca elementos con el atributo [name]
     */
    static getFormData(containerSelector) {
        const container = this.$(containerSelector);
        const data = {};
        container.querySelectorAll("[name]").forEach(el => {
            const val = el.type === 'checkbox' ? el.checked : el.value;
            data[el.getAttribute("name")] = val;
        });
        return data;
    }

    /**
     * Llena un formulario automáticamente a partir de un objeto JSON
     */
    static setFormData(containerSelector, data) {
        const container = this.$(containerSelector);
        Object.keys(data).forEach(key => {
            const el = container.querySelector(`[name="${key}"]`);
            if (el) {
                if (el.type === 'checkbox') el.checked = data[key];
                else el.value = data[key];
            }
        });
    }

    /**
     * Renderiza los errores en el campo de forma inteligente
     * @param {HTMLElement} inputEl - El input que falló
     * @param {string[]} mensajes - Array de mensajes de error
     */
    static #renderErrors(inputEl, mensajes) {
        if (!inputEl || mensajes.length === 0) return;

        inputEl.classList.add("is-invalid");

        const container = document.createElement("div");
        container.className = "error-container"; // Clase de tu SCSS

        const mainError = document.createElement("span");
        mainError.className = "error-msg-main"; // Clase de tu SCSS
        mainError.innerText = mensajes[0];
        container.appendChild(mainError);

        if (mensajes.length > 1) {
            const badge = document.createElement("span");
            badge.className = "error-badge-more"; // Clase de tu SCSS
            badge.innerText = `+${mensajes.length - 1}`;

            const popover = document.createElement("div");
            popover.className = "error-popover"; // Clase de tu SCSS
            popover.innerHTML = `<b>Errores:</b><ul>${mensajes.map(m => `<li>${m}</li>`).join('')}</ul>`;

            badge.appendChild(popover);
            container.appendChild(badge);
        }

        // Insertar despu�s del input moderno
        inputEl.parentNode.insertBefore(container, inputEl.nextSibling);
    }
    /**
   * Valida un formulario basado en un esquema de reglas
   * @param {string} containerSelector - ID o Clase del contenedor
   * @param {object} schema - Objeto con los nombres de campos y sus reglas
   * @returns {boolean} - true si es v�lido, false si fall�
   */
    static validateForm(containerSelector, schema) {
        const container = this.$(containerSelector);
        const data = this.getFormData(containerSelector);
        let isValid = true;

        // Limpiar previos
        container.querySelectorAll(".is-invalid").forEach(el => el.classList.remove("is-invalid"));
        container.querySelectorAll(".error-container").forEach(el => el.remove());

        for (const fieldName in schema) {
            const rules = schema[fieldName];
            const value = data[fieldName];
            const inputEl = container.querySelector(`[name="${fieldName}"]`);
            const fieldErrors = [];

            for (const rule of rules) {
                const result = rule(value);
                if (result !== true) {
                    fieldErrors.push(result);
                    isValid = false;
                }
            }

            if (fieldErrors.length > 0) {
                this.#renderErrors(inputEl, fieldErrors);
            }
        }
        return isValid;
    }

    // M�todo para que CoreService tambi�n use esta l�gica al recibir errores del Back
    static showServerErrors(erroresCampos) {
        for (const campo in erroresCampos) {
            const input = document.querySelector(`[name="${campo}"]`);
            this.#renderErrors(input, erroresCampos[campo]);
        }
    }

    /**
     * Limpia todos los inputs de un contenedor
     */
    static clearForm(containerSelector) {
        const container = this.$(containerSelector);
        container.querySelectorAll("[name]").forEach(el => {
            if (el.type === 'checkbox') el.checked = false;
            else el.value = "";
        });
    }
}