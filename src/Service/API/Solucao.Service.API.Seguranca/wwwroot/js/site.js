class KeyValueCultureCollections {
    constructor(settings) {
        this.collections = settings;
        this.init();
    }

    init() {
        this.collections.forEach(collection => {
            const container = document.getElementById(collection.container);
            const dataHiddenId = collection.dataHidden;
            const initData = collection.initData && collection.initData.length > 0 ? collection.initData : [];
            const cultures = collection.cultures || [];
            const readOnly = collection.readOnly;
            const truncateLimit = collection.truncateLimit !== undefined ? collection.truncateLimit : 0;

            const keyValueCultureCollection = new KeyValueCultureCollection(container, dataHiddenId, initData, cultures, readOnly, truncateLimit);
            keyValueCultureCollection.render();
        });
    }
}

class KeyValueCultureCollection {
    constructor(container, dataHiddenId, initData, cultures, readOnly, truncateLimit) {
        this.container = container;
        this.dataHiddenId = dataHiddenId;
        this.data = initData;
        this.cultures = cultures;
        this.readOnly = readOnly;
        this.truncateLimit = truncateLimit;
    }

    updateHiddenInput() {
        const hiddenInput = document.getElementById(this.dataHiddenId);
        if (hiddenInput) {
            hiddenInput.value = JSON.stringify(this.data);
        } else {
            console.warn('O campo oculto não pôde ser encontrado.');
        }
    }

    render() {
        const addButton = `<button type="button" class="btn btn-primary add-button" ${this.readOnly ? 'disabled' : ''}><i class="fas fa-plus-square"></i></button>`;

        let cultureOptions = '<option value="">Selecione uma cultura</option>';
        this.cultures.forEach(culture => {
            cultureOptions += `<option value="${culture.key}">${culture.value}</option>`;
        });

        let listHTML = `
            <div class="key-value-list">
                <div class="row">
                    <div class="col col-md-12 input-group mb-3">
                        <select class="form-control culture-input" ${this.readOnly ? 'disabled' : ''}>
                            ${cultureOptions}
                        </select>
                        <input type="text" class="form-control value-input" placeholder="Valor"  ${this.readOnly ? 'disabled' : ''}>
                        ${addButton}
                    </div>
                </div>
            </div>
        `;

        if (this.data && this.data.length > 0) {
            listHTML += `<div class="row key-value-list-items m-0 p-0 ">`;
            
            this.data.forEach((item, index) => {
                const culture = Object.keys(item)[0];
                const translation = item[culture];
                const truncatedTranslation = translation.length > this.truncateLimit ? translation.substring(0, this.truncateLimit) + '...' : translation;

                const listItemHTML = `
                    <div class="key-value-list-items-row input-group m-0 p-0 mb-1 ${index % 2 === 0 ? 'even' : 'odd'}">
                            <div class="key-value-list-items-column-key" ${this.readOnly ? 'disabled' : ''}>
                                ${this.cultures.find(c => c.key === culture)?.value || ''}
                            </div>
                            <div class="key-value-list-items-column-value">${truncatedTranslation}</div>
                            <button type="button" class="btn btn-danger remove-button" ${this.readOnly ? 'disabled' : ''}><i class="fa fa-trash"></i></button>
                    </div>
                `;

                listHTML += listItemHTML;
            });
            listHTML += `</div>`;
        }



        this.container.innerHTML = listHTML;

        // Verifica se o campo hidden existe
        let hiddenInput = document.querySelector(`#${this.dataHiddenId}`);
        if (!hiddenInput) {
            console.warn('O ID do input hidden não foi fornecido. Um novo campo hidden será criado.');
            // Cria um novo campo hidden dentro do form pai
            hiddenInput = document.createElement('input');
            hiddenInput.setAttribute('type', 'hidden');
            hiddenInput.setAttribute('id', this.dataHiddenId);
            const formParent = this.container.closest('form');
            if (formParent) {
                formParent.appendChild(hiddenInput);
            } else {
                console.error('Não foi possível encontrar o elemento form pai.');
            }
        }

        if (hiddenInput && this.data && this.data.length > 0) {
            hiddenInput.value = JSON.stringify(this.data);
        } else {
            hiddenInput.value = JSON.stringify([]);
        }

        const addButtonElement = this.container.querySelector('.add-button');
        if (addButtonElement) {
            addButtonElement.addEventListener('click', () => {
                const cultureInput = this.container.querySelector('.culture-input');
                const valueInput = this.container.querySelector('.value-input');
                const culture = cultureInput.value.trim();
                const value = valueInput.value.trim();

                if (culture && value) {
                    this.data.push({ [culture]: value });
                    this.updateHiddenInput();
                    this.render();
                }

                cultureInput.value = '';
                valueInput.value = '';
            });
        }

        const removeButtons = this.container.querySelectorAll('.remove-button');
        removeButtons.forEach(button => {
            button.addEventListener('click', () => {
                const keyValueItem = button.closest('.key-value-list-items');
                const keyValueIndex = Array.from(this.container.querySelectorAll('.key-value-list-items')).indexOf(keyValueItem);

                if (keyValueIndex > -1) {
                    this.data.splice(keyValueIndex, 1);
                    this.updateHiddenInput();
                    this.render();
                }
            });
        });
    }
}

class KeyValueCollections {
    constructor(settings) {
        this.collections = settings;
        this.init();
    }

    init() {
        this.collections.forEach(collection => {
            const container = document.getElementById(collection.container);
            const dataHiddenId = collection.dataHidden;
            const initData = collection.initData && collection.initData.length > 0 ? collection.initData : [];
            const readOnly = collection.readOnly;
            const truncateLimit = collection.truncateLimit !== undefined ? collection.truncateLimit : 0;

            const keyValueCollection = new KeyValueCollection(container, dataHiddenId, initData, readOnly, truncateLimit);
            keyValueCollection.render();
        });
    }
}

class KeyValueCollection {
    constructor(container, dataHiddenId, initData, readOnly, truncateLimit) {
        this.container = container;
        this.dataHiddenId = dataHiddenId;
        this.data = initData;
        this.readOnly = readOnly;
        this.truncateLimit = truncateLimit;
    }

    updateHiddenInput() {
        const hiddenInput = document.getElementById(this.dataHiddenId);
        if (hiddenInput) {
            hiddenInput.value = JSON.stringify(this.data);
        } else {
            console.warn('O campo oculto não pôde ser encontrado.');
        }
    }

    render() {
        const addButton = `<button type="button" class="btn btn-primary add-button" ${this.readOnly ? 'disabled' : ''}><i class="fas fa-plus-square"></i></button>`;
        let listHTML = `
            <div class="key-value-list">
                <div class="row">
                    <div class="col col-md-12 input-group mb-3">
                        <input type="text" class="form-control key-input" placeholder="Chave"  ${this.readOnly ? 'disabled' : ''}>
                        <input type="text" class="form-control value-input" placeholder="Valor"  ${this.readOnly ? 'disabled' : ''}>
                        ${addButton}
                    </div>
                </div>
            </div>
        `;

        if (this.data && this.data.length > 0) {
            listHTML += `<div class="row key-value-list-items m-0 p-0">`;
            this.data.forEach((item, index) => {
                const key = Object.keys(item)[0];
                const value = item[key];
                let truncatedKey = key;
                let truncatedValue = value;

                if (this.truncateLimit > 0) {
                    truncatedKey = key.length > this.truncateLimit ? key.substring(0, this.truncateLimit) + '...' : key;
                    truncatedValue = value.length > this.truncateLimit ? value.substring(0, this.truncateLimit) + '...' : value;
                }

                listHTML += `
                    <div class="key-value-list-items-row input-group m-0 p-0 mb-1 ${index % 2 === 0 ? 'even' : 'odd'}">
                        <div class="key-value-list-items-column-value text-truncate" title="${key}">${truncatedKey}</div>
                        <div class="key-value-list-items-column-value text-truncate" title="${value}">${truncatedValue}</div>
                        <button type="button" class="btn btn-danger remove-button" ${this.readOnly ? 'disabled' : ''}><i class="fa fa-trash"></i></button>
                    </div>
                `;
            });
            listHTML += `</div>`;
        }

        this.container.innerHTML = listHTML;

        let hiddenInput = document.querySelector(`#${this.dataHiddenId}`);
        if (!hiddenInput) {
            console.warn('O ID do input hidden não foi fornecido. Um novo campo hidden será criado.');
            hiddenInput = document.createElement('input');
            hiddenInput.setAttribute('type', 'hidden');
            hiddenInput.setAttribute('id', this.dataHiddenId);
            const formParent = this.container.closest('form');
            if (formParent) {
                formParent.appendChild(hiddenInput);
            } else {
                console.error('Não foi possível encontrar o elemento form pai.');
            }
        }

        if (hiddenInput && this.data && this.data.length > 0) {
            hiddenInput.value = JSON.stringify(this.data);
        } else {
            hiddenInput.value = JSON.stringify([]);
        }

        const addButtonElement = this.container.querySelector('.add-button');
        if (addButtonElement) {
            addButtonElement.addEventListener('click', () => {
                const keyInput = this.container.querySelector('.key-input');
                const valueInput = this.container.querySelector('.value-input');
                const key = keyInput.value.trim();
                const value = valueInput.value.trim();

                if (key && value) {
                    this.data.push({ [key]: value });
                    this.updateHiddenInput();
                    this.render();
                }

                keyInput.value = '';
                valueInput.value = '';
            });
        }

        const removeButtons = this.container.querySelectorAll('.remove-button');
        removeButtons.forEach(button => {
            button.addEventListener('click', () => {
                const keyValueItem = button.closest('.key-value-list-items');
                const keyValueIndex = Array.from(this.container.querySelectorAll('.key-value-list-items')).indexOf(keyValueItem);

                if (keyValueIndex > -1) {
                    this.data.splice(keyValueIndex, 1);
                    this.updateHiddenInput();
                    this.render();
                }
            });
        });
    }
}

class StringCollections {
    constructor(settings) {
        this.collections = settings;
        this.init();
    }

    init() {
        this.collections.forEach(collection => {
            const container = document.getElementById(collection.container);
            const dataHiddenId = collection.dataHidden;
            const initData = collection.initData && collection.initData.length > 0 ? collection.initData : [];
            const readOnly = collection.readOnly;
            const truncateLimit = collection.truncateLimit !== undefined ? collection.truncateLimit : 0;
            const dataTips = collection.dataTips && collection.dataTips.length > 0 ? collection.dataTips : [];

            const stringCollection = new StringCollection(container, dataHiddenId, initData, readOnly, truncateLimit, dataTips);
            stringCollection.render();
        });
    }
}

class StringCollection {
    constructor(container, dataHiddenId, initData, readOnly, truncateLimit, dataTips) {
        this.container = container;
        this.dataHiddenId = dataHiddenId;
        this.data = initData;
        this.readOnly = readOnly;
        this.truncateLimit = truncateLimit;
        this.dataTips = dataTips;
    }

    updateHiddenInput() {
        const hiddenInput = document.getElementById(this.dataHiddenId);
        if (hiddenInput) {
            hiddenInput.value = JSON.stringify(this.data);
        } else {
            console.warn('O campo oculto não pôde ser encontrado.');
        }
    }

    render() {
        const addButton = `<button type="button" class="btn btn-primary add-button" ${this.readOnly ? 'disabled' : ''}><i class="fas fa-plus-square"></i></button>`;
        let listHTML = `
            <div class="string-list mb-3">
                <div class="row">
                    <div class="col col-md-12 input-group mb-3">
                        <input class="form-control string-input" list="datalistOptions_${this.dataHiddenId}" placeholder="Procurar..." ${this.readOnly ? 'disabled' : ''}>
                        <datalist id="datalistOptions_${this.dataHiddenId}">
                            ${this.dataTips.map(tip => `<option value="${tip}">`).join('')}
                        </datalist>
                        ${addButton}
                    </div>
                </div>
            </div>
        `;

        if (this.data && this.data.length > 0) {
            listHTML += `<div class="row string-list-items m-0 p-0">`;
            this.data.forEach((string, index) => {
                listHTML += `
                    <div class="string-list-items-row input-group m-0 p-0 mb-1 ${index % 2 === 0 ? 'even' : 'odd'}">
                        <div class="string-list-items-column-value text-truncate" title="${string}">${string}</div>
                        <button type="button" class="btn-danger remove-button" ${this.readOnly ? 'disabled' : ''}><i class="fa fa-trash"></i></button>
                    </div>
                `;
            });
            listHTML += `</div>`;
        }

        this.container.innerHTML = listHTML;

        // Verifica se o campo hidden existe
        let hiddenInput = document.getElementById(this.dataHiddenId);
        if (!hiddenInput) {
            console.warn('O ID do input hidden não foi fornecido. Um novo campo hidden será criado.');
            // Cria um novo campo hidden dentro do form pai
            hiddenInput = document.createElement('input');
            hiddenInput.setAttribute('type', 'hidden');
            hiddenInput.setAttribute('id', this.dataHiddenId);
            const formParent = this.container.closest('form');
            if (formParent) {
                formParent.appendChild(hiddenInput);
            } else {
                console.error('Não foi possível encontrar o elemento form pai.');
            }
        }

        if (hiddenInput && this.data && this.data.length > 0) {
            hiddenInput.value = JSON.stringify(this.data);
        } else {
            hiddenInput.value = JSON.stringify([]);
        }

        const addButtonElement = this.container.querySelector('.add-button');
        if (addButtonElement) {
            addButtonElement.addEventListener('click', () => {
                const stringInput = this.container.querySelector('.string-input');
                const inputString = stringInput.value.trim().toLowerCase();

                if (inputString && !this.data.some(str => str.toLowerCase() === inputString)) {
                    this.data.push(inputString);
                    this.updateHiddenInput();
                    this.render();
                }

                stringInput.value = '';
            });
        }

        const removeButtons = this.container.querySelectorAll('.remove-button');
        removeButtons.forEach(button => {
            button.addEventListener('click', () => {
                const stringItem = button.closest('.string-list-items');
                const stringIndex = Array.from(this.container.querySelectorAll('.string-list-items')).indexOf(stringItem);

                if (stringIndex > -1) {
                    this.data.splice(stringIndex, 1);
                    this.updateHiddenInput();
                    this.render();
                }
            });
        });
    }
}



