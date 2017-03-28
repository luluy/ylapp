//时间加(减)天
   Date.prototype.addDay = function (d) {
     var m = this.getTime() + d * 60 * 60 * 1000*24;
     return new Date(m);
     //this.setHours(this.getHours() + parseInt(h));
   };
   //对Date的扩展，将 Date 转化为指定格式的String    
   //月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符，    
   //年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字)    
   //例子：    
   //(new Date()).Format("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423    
   //(new Date()).Format("yyyy-M-d h:m:s.S")      ==> 2006-7-2 8:9:4.18
   Date.prototype.Format = function (fmt) { //author: meizz    
       var o = {
           "M+": this.getMonth() + 1,                 //月份    
           "d+": this.getDate(),                    //日    
           "h+": this.getHours(),                   //小时    
           "m+": this.getMinutes(),                 //分    
           "s+": this.getSeconds(),                 //秒    
           "q+": Math.floor((this.getMonth() + 3) / 3), //季度    
           "S": this.getMilliseconds()             //毫秒    
       };
       if (/(y+)/.test(fmt))
           fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
       for (var k in o)
           if (new RegExp("(" + k + ")").test(fmt))
               fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
       return fmt;
   }
   
   function parseDate(str) // 这个函数用来把字符串转换为日期格式
   {
       return new Date(Date.parse(str.replace(/-/g,"/")));
   }

   /**
   * 将HH:MM转为秒
   */
   function formatSeconds(value) {
       var time = value.split(":");
       result = parseInt(time[0]) * 3600 + parseInt(time[1]) * 60;
       return result;
   }
   function CalDateSub(d1, d2) {
       return (d1.getTime() - d2.getTime()) / 1000;
   }
   /**
   * @function 将时间戳转化为日+小时+分+秒
   * @param {Date} 时间戳
   * @return {String} 时间字符串
   */
   function formatTime(longTime) {
       //转化为 日+小时+分+秒
       var time = parseFloat(longTime);
       if (time != null && time != "") {
           var h = parseInt(time / 3600);
           var m = parseInt(time % 3600 / 60);
           if (h < 10) h = '0' + h;
           if (m < 10) m = '0' + m;
           time = h + ":" + m;
       }
       return time;
   }