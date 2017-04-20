// Write your Javascript code.
$(document).ready(function () {
    var numberOfPlayers = $("#selectPlayers").val();
    setPlayerTable($("#selectPlayers").val());
    setBack9PlayerNames();

    setCardForHoles($("#selectHoles").val());

    $("#selectHoles").change(function () {
        var selection = $(this).val();
        setCardForHoles(selection);
        setBack9PlayerNames();
    });

    $("#selectPlayers").change(function(){
        var selection = $(this).val();
        setPlayerTable(selection);
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

    $('#skinsOption').click(function () {
        $('#skinsWagerField').toggle(this.checked);
    });

    $('#nassauOption').click(function () {
        $('#nassauWagerField').toggle(this.checked);
    });

    function setCardForHoles(p1){
        if (p1 == '9') {
            $("#front9").show("fast");
            $("#back9").hide("fast");
        }
        if (p1 == '18') {
            $("#front9").show("fast");
            $("#back9").show("fast");
        }
    }

    function setPlayerTable(p1) {
        if (p1 == 2) {
            $(".player1").show("fast");
            $(".player2").show("fast");
            $(".player3").hide("fast");
            $(".player4").hide("fast");
        }
        if (p1 == 3) {
            $(".player1").show("fast");
            $(".player2").show("fast");
            $(".player3").show("fast");
            $(".player4").hide("fast");

        }
        if (p1 == 4) {
            $(".player1").show("fast");
            $(".player2").show("fast");
            $(".player3").show("fast");
            $(".player4").show("fast");
        }
    }

    function setBack9PlayerNames(){
        $('#player1Label').html($('#player1NameInput').val());
        $('#player2Label').html($('#player2NameInput').val());
        $('#player3Label').html($('#player3NameInput').val());
        $('#player4Label').html($('#player4NameInput').val());
    }
});