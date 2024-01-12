var tooltip = {
  /**
                * Make a Tooltip
                **/
  make(target, content, orientation = 'right', type = 'help') {
    return new Tooltip({
      target: document.querySelector(target),
      content: content,
      classes: `tooltip ${type}-${orientation}`,
      position: `${orientation} middle` });

  },

  /**
     * Help tooltip
     **/
  help(t, c, o = 'right') {
    return this.make(t, c, o, "help");
  },

  /**
     * Info Tooltip
     **/
  info(t, c, o = 'right') {
    return this.make(t, c, o, "info");
  } };


var input1Tooltip = tooltip.make(".input1", "content");
var input2Tooltip = tooltip.help(".input2", "content2", "left");
var input3Tooltip = tooltip.help(".input3", "Cette tooltips est a <b>droite!</b>");

var input4Tooltip = tooltip.make(".input4", "Ceci est une information", "right", "info");
var input5Tooltip = tooltip.info(".input5", "Une info a gauche", "left");
var input6Tooltip = tooltip.info(".input6", "last info<br>newline super longue de la mort qui tue<br>newline super longue de la mort qui tue<br>newline super longue de la mort qui tue<br>newline super longue de la mort qui tue");