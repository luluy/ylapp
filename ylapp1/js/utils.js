/*实用工具类*/
/*字符串类*/
//格式化字符串 %1%2
//调用方式 String.format("%1%2",xx,xx)
String.format = function (string) {
    var args = arguments;
    var pattern = new RegExp("%([0-9]+)", "g");
    return String(string).replace(pattern, function (match, index) {
        if (index == 0 || index >= args.length)
            throw "Invalid index in format string";
        return args[index];
    }
	);
};
//取url的参数,根据名称得到值
var QueryString = function (name) {
    var reg = new RegExp("(^|&|\\?)" + name + "=([^&]*)(&|$)"), r;
    if (r = window.location.search.match(reg)) return unescape(r[2]);
    return "";
};

/*数据类*/
function HashMap() {
    //定义长度  
    var length = 0;
    //创建一个对象  
    var obj = new Object();

    /** 
    * 判断Map是否为空 
    */
    this.isEmpty = function () {
        return length == 0;
    };

    /** 
    * 判断对象中是否包含给定Key 
    */
    this.containsKey = function (key) {
        return (key in obj);
    };

    /** 
    * 判断对象中是否包含给定的Value 
    */
    this.containsValue = function (value) {
        for (var key in obj) {
            if (obj[key] == value) {
                return true;
            }
        }
        return false;
    };

    /** 
    *向map中添加数据 
    */
    this.put = function (key, value) {
        if (!this.containsKey(key)) {
            length++;
        }
        obj[key] = value;
    };

    /** 
    * 根据给定的Key获得Value 
    */
    this.get = function (key) {
        return this.containsKey(key) ? obj[key] : null;
    };

    this.set = function (key, val) {
        return this.containsKey(key) ? obj[key] = val : null;
    };

    /** 
    * 根据给定的Key删除一个值 
    */
    this.remove = function (key) {
        if (this.containsKey(key) && (delete obj[key])) {
            length--;
        }
    };

    /** 
    * 获得Map中的所有Value 
    */
    this.values = function () {
        var _values = new Array();
        for (var key in obj) {
            _values.push(obj[key]);
        }
        return _values;
    };

    //返回数组
    this.toArray = function () {
        var tmp = new Array();
        for (var key in obj) {
            tmp.push([parseInt(key), obj[key]]);  //为了highcharts的日期，做了类型转换，否则key为字符串类型
        }
        return tmp;
    };
    /** 
    * 获得Map中的所有Key 
    */
    this.keySet = function () {
        var _keys = new Array();
        for (var key in obj) {
            _keys.push(key);
        }
        return _keys;
    };

    /** 
    * 获得Map的长度 
    */
    this.size = function () {
        return length;
    };

    /** 
    * 清空Map 
    */
    this.clear = function () {
        length = 0;
        obj = new Object();
    };
}
//对象复制
var objClone = function (obj) {   //Object.prototype.Clone 跟jQuery有冲突，改为函数  
    var objClone;
    if (obj.constructor == Object) {
        objClone = new obj.constructor();
    } else {
        objClone = new obj.constructor(obj.valueOf());
    }
    for (var key in obj) {
        if (objClone[key] != obj[key]) {
            if (typeof (obj[key]) == 'object') {
                objClone[key] = obj[key].Clone();
            } else {
                objClone[key] = obj[key];
            }
        }
    }
    objClone.toString = obj.toString;
    objClone.valueOf = obj.valueOf;
    return objClone;
}
/*json对象或数组对象排序方法 name为要排序的字段，order表示排序方法 asc desc*/
var by = function (name, order) {
    return function (o, p) {
        var a, b;
        if (typeof o === "object" && typeof p === "object" && o && p) {
            a = o[name];
            b = p[name];
            if (a === b) { return 0; }
            if (order === 'desc') {
                if (typeof a === typeof b) { return a < b ? 1 : -1; }
                return typeof a < typeof b ? 1 : -1;
            } else if (order === 'asc') {
                if (typeof a === typeof b) { return a < b ? -1 : 1; }
                return typeof a < typeof b ? -1 : 1;
            }
        }
        else { throw ("error"); }
    }
}
/*数组最大、最小值*/
Array.prototype.max = function () {
    return Math.max.apply({}, this)
}
Array.prototype.min = function () {
    return Math.min.apply({}, this)
}
/*遍历对象数组，返回对应的对象 _list对象列表 property 对象的属性  value 属性值*/
function getObjByValue(_list,property, value) {
    var obj = null;
    if (_list.length > 0) {
        var i, len = _list.length;
        for (i = 0; i < len; i++) {
            if (_list[i][property] == value) {
                obj = _list[i];
                break;
            }
        }
    }
    return obj;
}
/*遍历对象数组，根据指定的key,修改相应的property值*/
function setObjValueByHey(_list, key, keyvalue, property, propvalue) {
    if (_list.length > 0) {
        var i, len = _list.length;
        for (i = 0; i < len; i++) {
            if (_list[i][key] == keyvalue) {
                //obj = _list[i];
                _list[i][property] = propvalue;
                break;
            }
        }
    }
}
