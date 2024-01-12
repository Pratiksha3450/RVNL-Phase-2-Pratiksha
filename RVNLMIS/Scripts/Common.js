$(document).ready(function () {
});

function NumbersOnly(evt) {
    var charCode = (evt.which) ? evt.which : evt.keyCode
    if (charCode > 31 && (charCode < 48 || charCode > 57))
        return false;
    return true;
}

function Numbers1Dot1MinusOnly(obj, e) {
    var dotCnt = $(obj).val().split('.').length;
    var minusCnt = $(obj).val().split('-').length;

    var unicode = e.charCode ? e.charCode : e.keyCode
    if (unicode != 8) {
        if ((unicode < 48 || unicode > 57) && unicode != 9 && unicode != 46 && unicode != 45) {
            return false;
        }
        else {
            if ((unicode == 46 && dotCnt == 2) || (unicode == 45 && minusCnt == 2)) {
                return false;
            }
            else
                return true;
        }
    }
}

function Numbers1DotOnly(obj, e) {
    var dotCnt = $(obj).val().split('.').length;

    var unicode = e.charCode ? e.charCode : e.keyCode
    if (unicode != 8) {
        if ((unicode < 48 || unicode > 57) && unicode != 9 && unicode != 46) {
            return false;
        }
        else {
            if ((unicode == 46 && dotCnt == 2)) {
                return false;
            }
            else
                return true;
        }
    }
}

function Numbers1PlusOnly(obj, e) {
    var plusCnt = $(obj).val().split('+').length;
    var specialKeys = new Array();
    specialKeys.push(8);
    specialKeys.push(43);

    var keyCode = e.which ? e.which : e.keyCode;
    if (keyCode != 37 && keyCode != 8 && keyCode != 46 && (keyCode >= 48 && keyCode <= 57) || specialKeys.indexOf(keyCode) != -1) {
        if ((specialKeys.indexOf(keyCode) != -1 && plusCnt == 2)) {
            return false;
        }
        else
            return true;
    }
    else {
        return false;
    }
}