function initializeSelect2(id, placeholdervalue, values, selectedValues) {
    var select = document.getElementById(id);

    // Adiciona os valores selecionados à lista de valores, se não existirem
    selectedValues.forEach(function (item) {
        if (!values.includes(item)) {
            values.push(item);
        }
    });

    // Transforma os valores em opções
    var options = values.map(function (item) {
        return { id: item, text: item, selected: selectedValues.includes(item) };
    });

    new Select2(select, {
        data: options,
        multiple: true,
        tags: true,
        maximumSelectionLength: 1000,
        tokenSeparators: [','],
        placeholder: placeholdervalue
    });
}

function Select2(element, options) {
    this.select = element;
    this.options = options;

    var self = this;
    setTimeout(function () {
        self.init();
    }, 0);
}

Select2.prototype.init = function () {
    var self = this;
    $(self.select).select2(self.options);
};