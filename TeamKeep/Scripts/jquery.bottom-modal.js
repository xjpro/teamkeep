$.widget("nmk.fadeAlert", {
    
    _timeout: null,
    _create: function() {
    },
    show: function (message, cssClass) {
        var me = this;
        if (me.timeout != null) {
            clearTimeout(me.timeout);
        }

        this.element.find(".alert").removeClass("alert-error alert-success alert-info");
        
        if (typeof cssClass !== "undefined") {
            this.element.find(".alert").addClass(cssClass);
        } else {
            this.element.find(".alert").addClass("alert-error");
        }
        
        this.element.find(".alert span").html(message);
        this.element.modal({ backdrop: false }).modal("show");
        
        me.timeout = setTimeout(function () {
            me.element.modal("hide");
            me.timeout = null;
        }, 3000);
    }
});

$.widget("nmk.bottomModal", {
   _create: function() {  
   },
   show: function () {
       if(this.element.is(":visible")) {
           this.element.effect("bounce", {}, 1000);
       } else {
           this.element.show({
               effect: "slide",
               easing: "easeOutBounce",
               direction: "down",
               duration: 1000
           });
       }
   },
   hide: function() {
       this.element.hide({
           effect: "slide",
           easing: "swing",
           direction: "down",
           duration: 500
       });
   }
});