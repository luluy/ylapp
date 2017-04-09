var sys_websocket = "ws://1.85.44.234:9612";
var sys_irri = {
    packet: {
        "ctp": 1,
        "uid": "admin",
        "utp": 1,
        "rtu": "17010076",
        "op": "37",
        "value": "01FF",
        "otp": 0,
        "broad": 0,
        "success": true,
        "message": "",
        "serial": 100,
        "tm": "2016-12-22 10:33:00",
        "data": null,
        toString: function () {
            that = this;
            return JSON.stringify(that);
        }
    }
};
var ws;
function fun_initWebSocket(a) {
	console.log("fun_initWebSocket");
    try {
        ws = new WebSocket(sys_websocket);
        ws.onopen = function (event) {
            console.log("已经与服务器建立了连接\r\n当前连接状态：" + this.readyState);
            //$(".networkstate").text("正常");
            /*登录*/
            console.log("正在登录....");
            ws.send(a);
        };
        ws.onmessage = function (event) {
            var rs = JSON.parse(event.data);
            //console.log(rs);
            var stcd = rs.rtu;
            switch (rs.op) {
                case 'EF':
                    if (rs.success) {
                        console.log("登录成功，可以操作！");

                    } else {
                        console.log("登录失败，请稍侯再试！");
                    }
                    break;
                case '4D': /*阀门*/
                    if (rs.success) {
                        //layer.closeAll();
                        var operate = rs.value;   //"0101"
                        var value = parseInt(operate.substr(2, 2));  //subtring(2,4);
                        var house = getObjByValue(stcdList, "STCD", stcd).CD;
                        if ((value & 1) == 1) {
                            //$("#" + house + "-1").attr("src", "images/valve-open.png");
                            //$("#" + house + "-1").attr("state", "1");
                        }
                        else if ((value & 1) == 0) {
                            //$("#" + house + "-1").attr("src", "images/valve-close.png");
                            //$("#" + house + "-1").attr("state", "0");
                        };
                        //layer.msg("操作成功！", { icon: 1, time: 3000 });
                        //window.clearTimeout(valvetimeout);
                    } else {
                        //layer.closeAll();
                        //layer.msg(rs.message, { icon: 2, time: 3000 });
                        mui.toast(rs.message);
                        //window.clearTimeout(valvetimeout);
                    }
                    break;
                case '4C': /*泵站*/
               		console.log(JSON.stringify(rs));
                    var operate = rs.value;   //"0101"
                    var value = parseInt(operate.substr(2, 2));  //subtring(2,4);
                    var code = getObjByValue(stcdList, "STCD", stcd).CD;
                    console.log(JSON.stringify(code));               
                    if (rs.success) {
                    	
                        //layer.closeAll();

                        setObjValueByHey(bumpList, "pcd", code + "-1", "run_cond", value);
                        setObjValueByHey(bumpList, "pcd", code + "-1", "tm", (new Date()).Format("yyyy-MM-dd hh:mm:ss"));
                        $($("#switch_state"+code+"-1")[0].parentNode.parentNode).children().children(".sb_run_cond").html(value == "1"?"开启" : "关闭");
                        
                        if(value == 1){
							removeClass($("#switch_state"+code+"-1")[0],"switch-close");
							addClass($("#switch_state"+code+"-1")[0],"switch-open");                        	
                        }else
                        {
							removeClass($("#switch_state"+code+"-1")[0],"switch-open");
							addClass($("#switch_state"+code+"-1")[0],"switch-close");                        	
                        }
                        //loadbz();
                        //$("#" + code + "-1-state").text(value == "1" ? "开启" : "关闭");
                        //$("#" + code + "-1-runtm").text("00:00");
                        //value == 1 ? $("#" + code + "-1").removeClass("close").addClass("open") : $("#" + code + "-1").removeClass("open").addClass("close");
                        //value == 1 ? $("#" + code + "-1 ul li").eq(0).addClass("on").siblings().removeClass("on") : $("#" + code + "-1 ul li").eq(1).addClass("on").siblings().removeClass("on");
                        //layer.msg("操作成功！", { icon: 1, time: 3000 });
                        mui.toast("操作成功！");
                        //window.clearTimeout(bumptimeout);
                    } else {
                        //layer.closeAll();
                        //layer.msg(rs.message, { icon: 2, time: 3000 });
                        mui.toast(rs.message);
                        //window.clearTimeout(bumptimeout);
                        console.log($("#switch_state"+code+"-1").html());
                        //$($("#switch_state"+code+"-1").children()[0]).attr("class",value == "1"?"mui-switch mui-switch-blue":"mui-switch mui-switch-blue mui-active");
                        isToggleMan = false;
                        mui("#switch"+code+"-1").switch().toggle();
                    }
                    break;
            }
            //var result = eval('(' + a.data + ')');
            //console.log(result.message.replace(/(\r\n)|(\n)/g, '<br/>'));
        };
        ws.onclose = function (event) {
            //$(".networkstate").text("断开");
            console.log("和服务器断开连接！");
        };
        ws.onerror = function (event) {
            //$(".networkstate").text("异常");
            console.log("WebSocket异常！");
        };
        //ws.connect();
    } catch (e) {
        //$(".networkstate").text("异常");
        console.log(e);
    }
}
function fun_close() {
    ws.close()
}
function fun_sendto(a) {
    if (ws && ws.readyState == 1) {
        ws.send(a);
    } else {
        console.log("数据为空或连接已经断开！");
    }
}