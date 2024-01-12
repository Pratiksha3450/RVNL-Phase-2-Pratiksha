$(document).ready(function () {
    BindClick();
    $(".clsTab").click(function () {
        var key = $(this).data('key');
        $('.tabul li').each(function () { $(this).removeClass('active') });
        $(this).parent().addClass('active');
        $.ajax({
            url: '/Config/GetTabContent',
            type: "Post",
            data: { TabClicked: key },
            success: function (result) {
                if (result != '') {
                    $("#DivTabContent").html(result);
                    setTimeout(function () {
                        BindClick();
                    }, 500);
                    return;
                }
            },
            error: function (response) {
                $.notify(response, { align: "center", verticalAlign: "top", close: "true", type: "danger" });
            }
        });
    });
});

function OnPhyProgressSuccess(res) {
    if (res.msg == "valerror") {
        $('.clsCreatePhysicalParam').html(res.html);
    }
    else {
        $("#PhyID").val('0');
        $("#PhysicalParamCode").val('');
        $("#PhysicalParamName").val('');
        $('.field-validation-error').val('');
        $("#tbodyListPhyProgress").html('');
        $("#tbodyListPhyProgress").html(res);
        $.notify("Data added successfully.", { align: "right", verticalAlign: "top", type: "success", background: "#20D67B", color: "#fff" });
    }
    setTimeout(function () {
        BindClick();
    }, 500);
}

function OnCashFlowCreateSuccess(res) {
    if (res.msg == "valerror") {
        $('.clsCreateCashFlow').html(res.html);
    }
    else {
        $("#CashID").val('0');
        $("#CashParameter").val('');
        $("#ProjectCurrency").val('');
        $('.field-validation-error').val('');
        $("#tbodyCashFlow").html('');
        $("#tbodyCashFlow").html(res);
        $.notify("Data added successfully.", { align: "right", verticalAlign: "top", type: "success", background: "#20D67B", color: "#fff" });
    }
    setTimeout(function () {
        BindClick();
    }, 500);
}
function OnKPICreateSuccess(res) {
    if (res.msg == "valerror") {
        $('.clsCreateKPI').html(res.html);
    }
    else {
        $("#KPID").val('0');
        $("#KpiSequence").val('');
        $("#KpiParameter").val('');
        $("#Remark").val('');
        $('.field-validation-error').val('');
        $("#tbodyKPI").html('');
        $("#tbodyKPI").html(res);
        $.notify("Data added successfully.", { align: "right", verticalAlign: "top", type: "success", background: "#20D67B", color: "#fff" });
    }
    setTimeout(function () {
        BindClick();
    }, 500);
}

function OnHSECreateSuccess(res) {
    if (res.msg == "valerror") {
        $('.clsCreateHSE').html(res.html);
    }
    else {
        $("#HPID").val('0');
        $("#HseParamSeq").val('');
        $("#HseParamName").val('');
        $('.field-validation-error').html('');
        $("#tbodyHSE").html('');
        $("#tbodyHSE").html(res);
        $.notify("Data added successfully.", { align: "right", verticalAlign: "top", type: "success", background: "#20D67B", color: "#fff" });
    }
    setTimeout(function () {
        BindClick();
    }, 500);
}

function OnQAQCCreateSuccess(res) {
    if (res.msg == "valerror") {
        $('.clsCreateQAQC').html(res.html);
    }
    else {
        $("#QPID").val('0');
        $("#QParSeq").val('');
        $("#QParName").val('');
        $('.field-validation-error').text('');
        $("#tbodyQAQC").html('');
        $("#tbodyQAQC").html(res);
        $.notify("Data added successfully.", { align: "right", verticalAlign: "top", type: "success", background: "#20D67B", color: "#fff" });
    }
    setTimeout(function () {
        BindClick();
    }, 500);
}

function OnBaselineCreateSuccess(res) {
    if (res.msg == "valerror") {
        $('.clsCreateBaseline').html(res.html);
    }
    else {
        $("#BaseID").val('0');
        $("#BaseRevision").val('');
        $("#BaseSubmissionDate").val('');
        $("#BaselineStatusId").val('');
        $("#ResponseDate").val('');
        $("#Remark").val('');
        $("#IsActive").prop('checked', false);
        $('.field-validation-error').html('');
        $("#tbodyBaseline").html('');
        $("#tbodyBaseline").html(res);
        $.notify("Data added successfully.", { align: "right", verticalAlign: "top", type: "success", background: "#20D67B", color: "#fff" });
    }
    setTimeout(function () {
        BindClick();
    }, 500);
}

function OnDuePeriodCreateSuccess(res) {
    if (res.msg == "valerror") {
        $('.clsCreateDuePeriods').html(res.html);
    }
    else {
        $("#InvDID").val('0');
        $("#ApprovalPeriod").val('');
        $("#PaymentPeriod").val('');
        $('.field-validation-error').text('');
        $("#tbodyDuePeriods").html('');
        $("#tbodyDuePeriods").html(res);
        $.notify("Data added successfully.", { align: "right", verticalAlign: "top", type: "success", background: "#20D67B", color: "#fff" });
    }
    setTimeout(function () {
        BindClick();
    }, 500);
}

function BindClick() {
    $(".EditById").click(function () {
        console.log('EditById clicked');
        var idToEdit = $(this).data("key");
        var WhichEditClicked = $(this).data("tab");
        console.log(WhichEditClicked);
        var editMethodUrl;
        var createDivClass;
        switch (WhichEditClicked) {
            case "EditPhysicalProgress":
                editMethodUrl = "/Config/EditPhyProgress";
                createDivClass = "clsCreatePhysicalParam";
                break;
            case "EditCashFlow":
                editMethodUrl = '/Config/EditCashFlow';
                createDivClass = "clsCreateCashFlow";
                break;
            case "EditKPI":
                editMethodUrl = "/Config/EditKPI";
                createDivClass = "clsCreateKPI";
                break;
            case "EditBaseline":
                editMethodUrl = "/Config/EditBaseline";
                createDivClass = "clsCreateBaseline";
                break;
            case "EditDuePeriod":
                editMethodUrl = "/Config/EditDuePeriod";
                createDivClass = "clsCreateDuePeriods";
                break;
            case "EditHistogram":
                editMethodUrl = "/Config/EditHistogram";
                createDivClass = "clsCreateHistogram";
                break;
            case "EditHSE":
                editMethodUrl = "/Config/EditHSE";
                createDivClass = "clsCreateHSE";
                break;
            case "EditQAQC":
                editMethodUrl = "/Config/EditQAQC";
                createDivClass = "clsCreateQAQC";
                break;
            default: break;
        }
        $.get(editMethodUrl, { "IdToEdit": idToEdit }, function (data) {
            $('.' + createDivClass).html(data);
        });
    });

    $(".ConfirmDelete").click(function () {
        console.log('delete clicked');
        var id = $(this).data('key');
        $("#deleteFromWhichTab").val($(this).data('tab'))
        console.log('id to delete ' + id);
        $("#hdnIdToDelete").val(id);
        $("#modalDeleteAttachment").modal({ backdrop: 'static', keyboard: false, position: 'center' });
    });
}

function DeleteRowByID() {
    var idToDelete = $("#hdnIdToDelete").val();
    var WhichDeleteClicked = $("#deleteFromWhichTab").val();
    var deleteMethodUrl, tbodyId;
    switch (WhichDeleteClicked) {
        case "DeletePhysicalProgress":
            deleteMethodUrl = '/Config/DeletePhyProgress';
            tbodyId = "tbodyListPhyProgress";
            break;
        case "DeleteCashFlow":
            deleteMethodUrl = '/Config/DeleteCashFlow';
            tbodyId = "tbodyCashFlow";
            break;
        case "DeleteKPI":
            deleteMethodUrl = '/Config/DeleteKPI';
            tbodyId = "tbodyKPI";
            break;
        case "DeleteBaseline":
            deleteMethodUrl = '/Config/DeleteBaseline';
            tbodyId = "tbodyBaseline";
            break;
        case "DeleteDuePeriod":
            deleteMethodUrl = '/Config/DeleteDuePeriod';
            tbodyId = "tbodyDuePeriods";
            break;
        case "DeleteHistogram":
            deleteMethodUrl = '/Config/DeleteHistogram';
            tbodyId = "tbodyHistogram";
            break;
        case "DeleteHSE":
            deleteMethodUrl = '/Config/DeleteHSE';
            tbodyId = "tbodyHSE";
            break;
        case "DeleteQAQC":
            deleteMethodUrl = '/Config/DeleteQAQC';
            tbodyId = "tbodyQAQC";
            break;
        default: break;
    }

    $.ajax({
        url: deleteMethodUrl,
        type: "Post",
        data: { id: idToDelete },
        success: function (result) {
            if (result != '') {
                $('#' + tbodyId).html('');
                $('#' + tbodyId).html(result);
                $.notify("Data deleted successfully.", { align: "right", verticalAlign: "top", type: "success", background: "#20D67B", color: "#fff" });
                $("#modalDeleteAttachment").modal('hide');
                setTimeout(function () {
                    BindClick();
                }, 500);
                return;
            }
            $.notify("Error", { align: "center", verticalAlign: "top", close: "true", type: "danger" });
            $("#modalDeleteAttachment").modal('hide');
        },
        error: function (response) {
            $.notify(response, { align: "center", verticalAlign: "top", close: "true", type: "danger" });
            console.log("Error", response);
            $("#modalDeleteAttachment").modal('hide');
        }
    });
}