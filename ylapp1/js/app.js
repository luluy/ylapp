/**
 * 演示程序当前的 “注册/登录” 等操作，是基于 “本地存储” 完成的
 * 当您要参考这个演示程序进行相关 app 的开发时，
 * 请注意将相关方法调整成 “基于服务端Service” 的实现。
 **/
(function($, owner) {
	/**
	 * 用户登录
	 **/
	owner.login = function(loginInfo, callback) {
		callback = callback || $.noop;
		loginInfo = loginInfo || {};
		loginInfo.account = loginInfo.account || '';
		loginInfo.password = loginInfo.password || '';
		if (loginInfo.account.length < 2) {
			return callback('账号最短为 2 个字符');
		}
		if (loginInfo.password.length < 2) {
			return callback('密码最短为 2 个字符');
		}
		
		plus.nativeUI.showWaiting("正在登录...");
		
		mui.ajax(app.loginurl(), {
			data:{ 'action': 'login', 'city': '', 'remember': 'sevenday', 'loginName': loginInfo.account, 'loginPwd': loginInfo.password },
			dataType: 'json', //服务器返回json格式数据
			type: 'get', //HTTP请求类型
			timeout: 60000, //超时时间设置为10秒；
			cache:false,
			success: function(data,status,xhr) {//服务器返回响应，根据响应结果，分析是否登录成功；
				plus.nativeUI.closeWaiting();
                if(data.success == true){
					return owner.createState(loginInfo.account, callback);
                }else{
                    return callback( data.msg);
                }
			},
			error: function(xhr, type, errorThrown) {
				//异常处理；
				plus.nativeUI.closeWaiting();
				plus.nativeUI.toast('网络遇到问题，请检测(' +type + ')');
				return callback(null);
			}
		});					
	};

	owner.createState = function(name, callback) {
		var state = owner.getState();
		state.account = name;
		state.token = "token123456789";
		owner.setState(state);
		return callback();
	};

	/**
	 * 新用户注册
	 **/
	owner.reg = function(regInfo, callback) {
		callback = callback || $.noop;
		regInfo = regInfo || {};
		regInfo.account = regInfo.account || '';
		regInfo.password = regInfo.password || '';
		if (regInfo.account.length < 5) {
			return callback('用户名最短需要 5 个字符');
		}
		if (regInfo.password.length < 6) {
			return callback('密码最短需要 6 个字符');
		}
		if (!checkEmail(regInfo.email)) {
			return callback('邮箱地址不合法');
		}
		var users = JSON.parse(localStorage.getItem('$users') || '[]');
		users.push(regInfo);
		localStorage.setItem('$users', JSON.stringify(users));
		return callback();
	};

	/**
	 * 获取当前状态
	 **/
	owner.getState = function() {
		var stateText = localStorage.getItem('$state') || "{}";
		return JSON.parse(stateText);
	};

	/**
	 * 设置当前状态
	 **/
	owner.setState = function(state) {
		state = state || {};
		localStorage.setItem('$state', JSON.stringify(state));
		//var settings = owner.getSettings();
		//settings.gestures = '';
		//owner.setSettings(settings);
	};

	var checkEmail = function(email) {
		email = email || '';
		return (email.length > 3 && email.indexOf('@') > -1);
	};

	/**
	 * 找回密码
	 **/
	owner.forgetPassword = function(email, callback) {
		callback = callback || $.noop;
		if (!checkEmail(email)) {
			return callback('邮箱地址不合法');
		}
		return callback(null, '新的随机密码已经发送到您的邮箱，请查收邮件。');
	};

	/**
	 * 获取应用本地配置
	 **/
	owner.setSettings = function(settings) {
		settings = settings || {};
		localStorage.setItem('$settings', JSON.stringify(settings));
	}

	/**
	 * 设置应用本地配置
	 **/
	owner.getSettings = function() {
			var settingsText = localStorage.getItem('$settings') || "{}";
			return JSON.parse(settingsText);
		}
		/**
		 * 获取本地是否安装客户端
		 **/
	owner.isInstalled = function(id) {
		if (id === 'qihoo' && mui.os.plus) {
			return true;
		}
		if (mui.os.android) {
			var main = plus.android.runtimeMainActivity();
			var packageManager = main.getPackageManager();
			var PackageManager = plus.android.importClass(packageManager)
			var packageName = {
				"qq": "com.tencent.mobileqq",
				"weixin": "com.tencent.mm",
				"sinaweibo": "com.sina.weibo"
			}
			try {
				return packageManager.getPackageInfo(packageName[id], PackageManager.GET_ACTIVITIES);
			} catch (e) {}
		} else {
			switch (id) {
				case "qq":
					var TencentOAuth = plus.ios.import("TencentOAuth");
					return TencentOAuth.iphoneQQInstalled();
				case "weixin":
					var WXApi = plus.ios.import("WXApi");
					return WXApi.isWXAppInstalled()
				case "sinaweibo":
					var SinaAPI = plus.ios.import("WeiboSDK");
					return SinaAPI.isWeiboAppInstalled()
				default:
					break;
			}
		}
	}
	
	
	
	
	owner.shoubu = {};
	owner.hosturl = "http://1.85.44.234/";
	owner.wsurl = "ws://1.85.44.234:9612sss";//var sys_websocket = "ws://1.85.44.234:9612";
	
	owner.loginurl = function(){return owner.hosturl + "admin/ashx/bg_user_login.ashx";}
	owner.c_a_yangling = function(){ return owner.hosturl + "control/ashx/yangling.ashx";}
	owner.gettaskurl = function(){ return owner.hosturl + "irriplan/ashx/bg_irriplan.ashx";}
	owner.bg_monitor = function(){ return owner.hosturl + "monitor/ashx/bg_monitor.ashx?action=getPUMPRealAllData";}
	owner.url_getfm = function(){ return owner.hosturl + "monitor/ashx/bg_monitor.ashx?action=getVALVERealAllData";}
	owner.bg_stat = function(){ return owner.hosturl + "bases/ashx/bg_stat.ashx?action=getSTCDRel";}
	//owner.loginurl = owner.hosturl + "admin/ashx/bg_user_login.ashx";	
	//owner.c_a_yangling = owner.hosturl + "control/ashx/yangling.ashx";
	//owner.gettaskurl = owner.hosturl + "irriplan/ashx/bg_irriplan.ashx";
	//owner.bg_monitor = owner.hosturl + "monitor/ashx/bg_monitor.ashx?action=getPUMPRealAllData";
	//owner.bg_stat = owner.hosturl + "bases/ashx/bg_stat.ashx?action=getSTCDRel";
	//console.log(owner.gettaskurl);
}(mui, window.app = {}));


function savedata(k,v){
	localStorage.setItem(k,JSON.stringify(v));
}

function getdata(k){
	return JSON.parse(localStorage.getItem(k));
}

function addClass(obj, cls){
	if(obj == null) return;
	if(!hasClass(obj,cls)){
	    var obj_class = obj.className,//获取 class 内容.
	    blank = (obj_class != '') ? ' ' : '';//判断获取到的 class 是否为空, 如果不为空在前面加个'空格'.
	    added = obj_class + blank + cls;//组合原来的 class 和需要添加的 class.
	    obj.className = added;//替换原来的 class.
   }
}

function removeClass(obj, cls){
	if(obj == undefined){ console.log("removeClass - obj is null");return;}
	while(hasClass(obj,cls)){
		removeClass1(obj,cls);
	}
	return;
}

 
function removeClass1(obj, cls){
	//console.log("removeClass" + obj + cls);
	if(obj == undefined){console.log("removeClass1 - obj is null"); return;}
    var obj_class = ' '+obj.className+' ';//获取 class 内容, 并在首尾各加一个空格. ex) 'abc        bcd' -> ' abc        bcd '
    obj_class = obj_class.replace(/(\s+)/gi, ' '),//将多余的空字符替换成一个空格. ex) ' abc        bcd ' -> ' abc bcd '
    removed = obj_class.replace(' '+cls+' ', ' ');//在原来的 class 替换掉首尾加了空格的 class. ex) ' abc bcd ' -> 'bcd '
    removed = removed.replace(/(^\s+)|(\s+$)/g, '');//去掉首尾空格. ex) 'bcd ' -> 'bcd'
    console.log("removeClass1 - new " + removed);
    obj.className = removed;//替换原来的 class.
}
 
function hasClass(obj, cls){
	if(obj == null) return;
    var obj_class = obj.className,//获取 class 内容.
    obj_class_lst = obj_class.split(/\s+/);//通过split空字符将cls转换成数组.
    x = 0;
    for(x in obj_class_lst) {
        if(obj_class_lst[x] == cls) {//循环数组, 判断是否包含cls
            return true;
        }
    }
    return false;
}
