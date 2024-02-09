document.addEventListener('DOMContentLoaded', function () {

    function observeDownloadUrlWrapper() {
        const observer = new MutationObserver((mutationsList) => {
            for (const mutation of mutationsList) {
                if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                    const downloadUrlWrapper = document.querySelector('.download-url-wrapper');
                    if (downloadUrlWrapper) {
                        observer.disconnect();
                        fetchDataAndCreateSelectors(downloadUrlWrapper);
                        updateLabels(downloadUrlWrapper);
                        break;
                    }
                }
            }
        });

        observer.observe(document.body, { childList: true, subtree: true });
    }

    function fetchDataAndCreateSelectors(downloadUrlWrapper) {
        fetch('/config/swaggerconfig.json')
            .then(response => response.json())
            .then(data => {
                if (data && data.themes && data.groups) {
                    createThemeSelector(downloadUrlWrapper, data.themes);
                }
            })
            .catch(error => {
                console.error('Erro ao obter dados do arquivo JSON:', error);
            });
    }

    function createThemeSelector(formWrapper, themes) {
        const themeSelector = createSelector('Tema', themes);

        const selectElement = themeSelector.querySelector('select');
        selectElement.addEventListener('change', function (event) {
            const selectedOptionValue = event.target.value;
            changeTheme(selectedOptionValue);
        });

        formWrapper.appendChild(themeSelector);
    }

    function changeTheme(newTheme) {
        const linkElement = document.querySelector('#csstheme');
        if (linkElement) {
            linkElement.href = newTheme;
        }
    }

    function updateLabels(downloadUrlWrapper) {
        const groupSpan = downloadUrlWrapper.querySelector('.select-label span');
        const themeSpan = downloadUrlWrapper.querySelector('.select-label:nth-of-type(2) span'); 

        if (groupSpan) {
            groupSpan.textContent = 'Grupo';
        }

        if (themeSpan) {
            themeSpan.textContent = 'Tema'; 
        }
    }

    function createSelector(labelText, options) {
        const selectLabel = document.createElement('label');
        selectLabel.classList.add('select-label');

        const span = document.createElement('span');
        span.textContent = labelText;

        const select = document.createElement('select');
        select.id = `select-${labelText.toLowerCase()}`;
        options.forEach(option => {
            const optionElement = document.createElement('option');
            optionElement.value = option.file || option.value;
            optionElement.textContent = option.name;
            select.appendChild(optionElement);
        });

        selectLabel.appendChild(span);
        selectLabel.appendChild(select);

        return selectLabel;
    }

    observeDownloadUrlWrapper();
});