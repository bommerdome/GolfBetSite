// Write your Javascript code.
$(document).ready(function () {
    var numberOfPlayers = $("#selectPlayers").val();


    $("#selectHoles").change(function () {
        var selection = $(this).val();
        if (selection == '9') {
            $("#front9").show("fast");
            $("#back9").hide("fast");
        }
        if (selection == '18') {
            $("#front9").show("fast");
            $("#back9").show("fast");
        }
    });

    $("#selectPlayers").change(function(){
        var selection = $(this).val();
        if (selection == 2) {
            $(".player1").show("fast");
            $(".player2").show("fast");
            $(".player3").hide("fast");
            $(".player4").hide("fast");
        }
        if (selection == 3) {
            $(".player1").show("fast");
            $(".player2").show("fast");
            $(".player3").show("fast");
            $(".player4").hide("fast");

        }
        if (selection == 4) {
            $(".player1").show("fast");
            $(".player2").show("fast");
            $(".player3").show("fast");
            $(".player4").show("fast");
        }

    });

    $('#selectPinWinner').change(function () {
        var selection = $(this).val();
        var idString = "#" + selection + "Skins";

        var x = $(idString);
        x = x + 1;
        $(idString).html(x);

    });

    $('#player1NameInput').change(function () {
        var name = $(this).val();
        $('#player1Label').html(name);
    });
    $('#player2NameInput').change(function () {
        var name = $(this).val();
        $('#player2Label').html(name);
    });
    $('#player3NameInput').change(function () {
        var name = $(this).val();
        $('#player3Label').html(name);
    });
    $('#player4NameInput').change(function () {
        var name = $(this).val();
        $('#player4Label').html(name);
    });

});