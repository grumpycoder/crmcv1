


$(function () {
    var $write = $('firstname'),
		shift = false,
		capslock = false;
    var index = 0;

    $('input').on('focus', function () {
        $write = $(this);
    });

    $('#keyboard li').click(function () {
        var $this = $(this),
			character = $this.html(); // If it's a lowercase letter, nothing happens to this variable

        // Shift keys
        if ($this.hasClass('left-shift') || $this.hasClass('right-shift')) {
            $('.letter').toggleClass('uppercase');
            $('.symbol span').toggle();

            shift = (shift === true) ? false : true;
            capslock = false;
            return false;
        }

        // Caps lock
        if ($this.hasClass('capslock')) {
            //            $('.letter').toggleClass('uppercase');
            //            capslock = true;
            return false;
        }

        // Delete
        if ($this.hasClass('delete')) {
            var html = $write.val();

            $write.val(html.substr(0, html.length - 1));
            return false;
        }

        // Special characters
        if ($this.hasClass('symbol')) character = $('span:visible', $this).html();
        if ($this.hasClass('space')) character = ' ';
        //        if ($this.hasClass('tab')) character = "\t";

        if ($this.hasClass('return')) character = "\n";

        if ($this.hasClass('tab')) {
            character = '';
            index += 1;
            if (index > $('input').length - 1) {
                index = 0;
            }
            $('input')[index].focus();
        };

        // Uppercase letter
        if ($this.hasClass('uppercase')) character = character.toUpperCase();

        // Remove shift once a key is clicked.
        if (shift === true) {
            $('.symbol span').toggle();
            if (capslock === false) $('.letter').toggleClass('uppercase');

            shift = false;
        }
        //        console.log("DO IT ", character);
        // Add the character
        $write.val($write.val() + character);
        $write.focus();
        //change event firing to force angular modle update
        $write.change();
    });

    //set focus on first input element in document
//        $('input')[0].focus();

    $('.letter').toggleClass('uppercase');
    capslock = true;

});
