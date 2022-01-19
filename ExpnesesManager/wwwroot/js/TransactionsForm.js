function initializeTransactionsForm(urlObtainCategories) {
    $("#OperationTypeId").change(async function () {
        const selectedValue = $(this).val();

        const reply = await fetch(urlObtainCategories, {
            method: "POST",
            body: selectedValue,
            headers: {
                "Content-Type": "application/json"
            }
        });

        const json = await reply.json()
        const options = json.map(category => `<option value=${category.value}>${category.text}</option>`);


        $("#CategoryId").html(options);
    })
}