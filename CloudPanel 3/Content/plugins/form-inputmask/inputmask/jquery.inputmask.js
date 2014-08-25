/*
 Input Mask plugin for jquery
 http://github.com/RobinHerbots/jquery.inputmask
 Copyright (c) 2010 - 2014 Robin Herbots
 Licensed under the MIT license (http://www.opensource.org/licenses/mit-license.php)
 Version: 3.1.3
*/
(function(f){"function"===typeof define&&define.amd?define(["jquery"],f):f(jQuery)})(function(f){if(void 0===f.fn.inputmask){var U=function(f){var e=document.createElement("input");f="on"+f;var d=f in e;d||(e.setAttribute(f,"return;"),d="function"==typeof e[f]);return d},D=function(c,e,d){return(c=d.aliases[c])?(c.alias&&D(c.alias,void 0,d),f.extend(!0,d,c),f.extend(!0,d,e),!0):!1},Q=function(c,e){function d(d){function f(d,e,c,g){this.matches=[];this.isGroup=d||!1;this.isOptional=e||!1;this.isQuantifier=
c||!1;this.isAlternator=g||!1;this.quantifier={min:1,max:1}}function e(d,f,g){var h=c.definitions[f],k=0==d.matches.length;g=void 0!=g?g:d.matches.length;if(h&&!q){for(var H=h.prevalidator,u=H?H.length:0,s=1;s<h.cardinality;s++){var v=u>=s?H[s-1]:[],r=v.validator,v=v.cardinality;d.matches.splice(g++,0,{fn:r?"string"==typeof r?RegExp(r):new function(){this.test=r}:/./,cardinality:v?v:1,optionality:d.isOptional,newBlockMarker:k,casing:h.casing,def:h.definitionSymbol||f,placeholder:h.placeholder,mask:f})}d.matches.splice(g++,
0,{fn:h.validator?"string"==typeof h.validator?RegExp(h.validator):new function(){this.test=h.validator}:/./,cardinality:h.cardinality,optionality:d.isOptional,newBlockMarker:k,casing:h.casing,def:h.definitionSymbol||f,placeholder:h.placeholder,mask:f})}else d.matches.splice(g++,0,{fn:null,cardinality:0,optionality:d.isOptional,newBlockMarker:k,casing:null,def:f,placeholder:void 0,mask:f}),q=!1}for(var k=/(?:[?*+]|\{[0-9\+\*]+(?:,[0-9\+\*]*)?\})\??|[^.?*+^${[]()|\\]+|./g,q=!1,s=new f,g,v=[],y=[],
u,h;g=k.exec(d);)switch(g=g[0],g.charAt(0)){case c.optionalmarker.end:case c.groupmarker.end:g=v.pop();if(0<v.length){if(u=v[v.length-1],u.matches.push(g),u.isAlternator){g=v.pop();for(u=0;u<g.matches.length;u++)g.matches[u].isGroup=!1;0<v.length?(u=v[v.length-1],u.matches.push(g)):s.matches.push(g)}}else s.matches.push(g);break;case c.optionalmarker.start:v.push(new f(!1,!0));break;case c.groupmarker.start:v.push(new f(!0));break;case c.quantifiermarker.start:u=new f(!1,!1,!0);g=g.replace(/[{}]/g,
"");h=g.split(",");g=isNaN(h[0])?h[0]:parseInt(h[0]);h=1==h.length?g:isNaN(h[1])?h[1]:parseInt(h[1]);if("*"==h||"+"==h)g="*"==h?0:1;u.quantifier={min:g,max:h};if(0<v.length){h=v[v.length-1].matches;g=h.pop();if(!g.isGroup){var z=new f(!0);z.matches.push(g);g=z}h.push(g);h.push(u)}else g=s.matches.pop(),g.isGroup||(z=new f(!0),z.matches.push(g),g=z),s.matches.push(g),s.matches.push(u);break;case c.escapeChar:q=!0;break;case c.alternatormarker:0<v.length?(u=v[v.length-1],h=u.matches.pop()):h=s.matches.pop();
h.isAlternator?v.push(h):(g=new f(!1,!1,!1,!0),g.matches.push(h),v.push(g));break;default:if(0<v.length){if(u=v[v.length-1],0<u.matches.length&&(h=u.matches[u.matches.length-1],h.isGroup&&(h.isGroup=!1,e(h,c.groupmarker.start,0),e(h,c.groupmarker.end))),e(u,g),u.isAlternator){g=v.pop();for(u=0;u<g.matches.length;u++)g.matches[u].isGroup=!1;0<v.length?(u=v[v.length-1],u.matches.push(g)):s.matches.push(g)}}else 0<s.matches.length&&(h=s.matches[s.matches.length-1],h.isGroup&&(h.isGroup=!1,e(h,c.groupmarker.start,
0),e(h,c.groupmarker.end))),e(s,g)}0<s.matches.length&&(h=s.matches[s.matches.length-1],h.isGroup&&(h.isGroup=!1,e(h,c.groupmarker.start,0),e(h,c.groupmarker.end)),y.push(s));return y}function y(e,k){if(c.numericInput&&!0!==c.multi){e=e.split("").reverse();for(var q=0;q<e.length;q++)e[q]==c.optionalmarker.start?e[q]=c.optionalmarker.end:e[q]==c.optionalmarker.end?e[q]=c.optionalmarker.start:e[q]==c.groupmarker.start?e[q]=c.groupmarker.end:e[q]==c.groupmarker.end&&(e[q]=c.groupmarker.start);e=e.join("")}if(void 0!=
e&&""!=e){if(0<c.repeat||"*"==c.repeat||"+"==c.repeat)e=c.groupmarker.start+e+c.groupmarker.end+c.quantifiermarker.start+("*"==c.repeat?0:"+"==c.repeat?1:c.repeat)+","+c.repeat+c.quantifiermarker.end;void 0==f.inputmask.masksCache[e]&&(f.inputmask.masksCache[e]={mask:e,maskToken:d(e),validPositions:{},_buffer:void 0,buffer:void 0,tests:{},metadata:k});return f.extend(!0,{},f.inputmask.masksCache[e])}}var z=[];f.isFunction(c.mask)&&(c.mask=c.mask.call(this,c));if(f.isArray(c.mask))if(e)f.each(c.mask,
function(d,e){void 0==e.mask||f.isFunction(e.mask)?z.push(y(e.toString())):z.push(y(e.mask.toString(),e))});else{c.keepStatic=void 0==c.keepStatic?!0:c.keepStatic;var q=!1,k="(";f.each(c.mask,function(d,e){1<k.length&&(k+=")|(");void 0==e.mask||f.isFunction(e.mask)?k+=e.toString():(q=!0,k+=e.mask.toString())});k+=")";z=y(k,q?c.mask:void 0)}else 1==c.mask.length&&!1==c.greedy&&0!=c.repeat&&(c.placeholder=""),z=void 0==c.mask.mask||f.isFunction(c.mask.mask)?y(c.mask.toString()):y(c.mask.mask.toString(),
c.mask);return z},ja="function"===typeof ScriptEngineMajorVersion?ScriptEngineMajorVersion():10<=(new Function("/*@cc_on return @_jscript_version; @*/"))(),w=navigator.userAgent,ka=null!==w.match(/iphone/i),la=null!==w.match(/android.*safari.*/i),ma=null!==w.match(/android.*chrome.*/i),na=null!==w.match(/android.*firefox.*/i),oa=/Kindle/i.test(w)||/Silk/i.test(w)||/KFTT/i.test(w)||/KFOT/i.test(w)||/KFJWA/i.test(w)||/KFJWI/i.test(w)||/KFSOWI/i.test(w)||/KFTHWA/i.test(w)||/KFTHWI/i.test(w)||/KFAPWA/i.test(w)||
/KFAPWI/i.test(w),Z=U("paste")?"paste":U("input")?"input":"propertychange",K=function(c,e,d){function y(a,b,n){b=b||0;var f=[],c,g=0,p;do{if(!0===a&&e.validPositions[g]){var l=e.validPositions[g];p=l.match;c=l.locator.slice();f.push(null==p.fn?p.def:!0===n?l.input:p.placeholder||d.placeholder.charAt(g%d.placeholder.length))}else c=b>g?L(g,c,g-1)[0]:w(g,c,g-1),p=c.match,c=c.locator.slice(),f.push(null==p.fn?p.def:void 0!=p.placeholder?p.placeholder:d.placeholder.charAt(g%d.placeholder.length));g++}while((void 0==
M||g-1<M)&&null!=p.fn||null==p.fn&&""!=p.def||b>=g);f.pop();return f}function z(a){var b=e;b.buffer=void 0;b.tests={};!0!==a&&(b._buffer=void 0,b.validPositions={},b.p=0)}function q(a){var b=-1,n=e.validPositions;void 0==a&&(a=-1);var d=b,f;for(f in n){var g=parseInt(f);if(-1==a||null!=n[g].match.fn)g<a&&(d=g),g>=a&&(b=g)}return 1<a-d||b<a?d:b}function k(a,b,n){if(d.insertMode&&void 0!=e.validPositions[a]&&void 0==n){n=f.extend(!0,{},e.validPositions);var g=q(),c;for(c=a;c<=g;c++)delete e.validPositions[c];
e.validPositions[a]=b;b=!0;for(c=a;c<=g;c++){a=n[c];if(void 0!=a){var x=null==a.match.fn?c+1:C(c);b=K(x,a.match.def)?b&&!1!==u(x,a.input,!0,!0):!1}if(!b)break}if(!b)return e.validPositions=f.extend(!0,{},n),!1}else e.validPositions[a]=b;return!0}function H(a,b){var d,f=a;for(d=a;d<b;d++)delete e.validPositions[d];for(d=b;d<=q();){var c=e.validPositions[d],g=e.validPositions[f];void 0!=c&&void 0==g?(K(f,c.match.def)&&!1!==u(f,c.input,!0)&&(delete e.validPositions[d],d++),f++):d++}for(d=q();0<d&&(void 0==
e.validPositions[d]||null==e.validPositions[d].match.fn);)delete e.validPositions[d],d--;z(!0)}function w(a,b,n){a=L(a,b,n);var c;b=q();b=e.validPositions[b]||L(0)[0];n=void 0!=b.alternation?b.locator[b.alternation].split(","):[];for(var g=0;g<a.length&&(c=a[g],!d.greedy&&(!c.match||!1!==c.match.optionality&&!1!==c.match.newBlockMarker||!0===c.match.optionalQuantifier||void 0!=b.alternation&&(void 0==c.locator[b.alternation]||-1!=f.inArray(c.locator[b.alternation].toString(),n))));g++);return c}function D(a){return e.validPositions[a]?
e.validPositions[a].match:L(a)[0].match}function K(a,b){for(var d=!1,f=L(a),e=0;e<f.length;e++)if(f[e].match&&f[e].match.def==b){d=!0;break}return d}function L(a,b,n){function c(b,n,g,h){function t(g,h,k){if(1E4<x)return alert("jquery.inputmask: There is probably an error in your mask definition or in the code. Create an issue on github with an example of the mask you are using. "+e.mask),!0;if(x==a&&void 0==g.matches)return p.push({match:g,locator:h.reverse()}),!0;if(void 0!=g.matches)if(g.isGroup&&
!0!==k){if(g=t(b.matches[m+1],h))return!0}else if(g.isOptional){var q=g;if(g=c(g,n,h,k))g=p[p.length-1].match,(g=0==f.inArray(g,q.matches))&&(l=!0),x=a}else if(g.isAlternator){var q=g,u=[],s,v=p.slice(),r=h.length,N=0<n.length?n.shift():-1;if(-1==N||"string"==typeof N){var z=x,y=n.slice(),ba;"string"==typeof N&&(ba=N.split(","));for(var w=0;w<q.matches.length;w++){p=[];g=t(q.matches[w],[w].concat(h),k)||g;s=p.slice();x=z;p=[];for(var A=0;A<y.length;A++)n[A]=y[A];for(A=0;A<s.length;A++)for(var B=s[A],
H=0;H<u.length;H++){var C=u[H];if(B.match.mask==C.match.mask&&("string"!=typeof N||-1!=f.inArray(B.locator[r].toString(),ba))){s.splice(A,1);C.locator[r]=C.locator[r]+","+B.locator[r];C.alternation=r;break}}u=u.concat(s)}"string"==typeof N&&(u=f.map(u,function(a,b){if(isFinite(b)){var d=a.locator[r].toString().split(","),n;a.locator[r]=void 0;a.alternation=void 0;for(var e=0;e<d.length;e++)if(n=-1!=f.inArray(d[e],ba))void 0!=a.locator[r]?(a.locator[r]+=",",a.alternation=r,a.locator[r]+=d[e]):a.locator[r]=
parseInt(d[e]);if(void 0!=a.locator[r])return a}}));p=v.concat(u);l=!0}else g=t(q.matches[N],[N].concat(h),k);if(g)return!0}else if(g.isQuantifier&&!0!==k)for(q=g,d.greedy=d.greedy&&isFinite(q.quantifier.max),k=0<n.length&&!0!==k?n.shift():0;k<(isNaN(q.quantifier.max)?k+1:q.quantifier.max)&&x<=a;k++){if(u=b.matches[f.inArray(q,b.matches)-1],g=t(u,[k].concat(h),!0))if(g=p[p.length-1].match,g.optionalQuantifier=k>q.quantifier.min-1,g=0==f.inArray(g,u.matches))if(k>q.quantifier.min-1){l=!0;x=a;break}else return!0;
else return!0}else{if(g=c(g,n,h,k))return!0}else x++}for(var m=0<n.length?n.shift():0;m<b.matches.length;m++)if(!0!==b.matches[m].isQuantifier){var k=t(b.matches[m],[m].concat(g),h);if(k&&x==a)return k;if(x>a)break}}var g=e.maskToken,x=b?n:0;n=b||[0];var p=[],l=!1;if(void 0==b){b=a-1;for(var h;void 0==(h=e.validPositions[b])&&-1<b;)b--;if(void 0!=h&&-1<b)x=b,n=h.locator.slice();else{for(b=a-1;void 0==(h=e.tests[b])&&-1<b;)b--;void 0!=h&&-1<b&&(x=b,n=h[0].locator.slice())}}for(b=n.shift();b<g.length&&
!(c(g[b],n,[b])&&x==a||x>a);b++);(0==p.length||l)&&p.push({match:{fn:null,cardinality:0,optionality:!0,casing:null,def:""},locator:[]});e.tests[a]=f.extend(!0,[],p);return e.tests[a]}function s(){void 0==e._buffer&&(e._buffer=y(!1,1));return e._buffer}function g(){void 0==e.buffer&&(e.buffer=y(!0,q(),!0));return e.buffer}function v(a,b){var n=g().slice();if(!0===a)z(),a=0,b=n.length;else for(var f=a;f<b;f++)delete e.validPositions[f],delete e.tests[f];for(f=a;f<b;f++)n[f]!=d.skipOptionalPartCharacter&&
u(f,n[f],!0,!0)}function Q(a,b){switch(b.casing){case "upper":a=a.toUpperCase();break;case "lower":a=a.toLowerCase()}return a}function u(a,b,n,c){function t(a,b,n,c){var l=!1;f.each(L(a),function(p,h){var F=h.match,x=b?1:0,t="";g();for(var m=F.cardinality;m>x;m--)t+=void 0==e.validPositions[a-(m-1)]?W(a-(m-1)):e.validPositions[a-(m-1)].input;b&&(t+=b);l=null!=F.fn?F.fn.test(t,e,a,n,d):b!=F.def&&b!=d.skipOptionalPartCharacter||""==F.def?!1:{c:F.def,pos:a};if(!1!==l){x=void 0!=l.c?l.c:b;x=x==d.skipOptionalPartCharacter&&
null===F.fn?F.def:x;t=a;void 0!=l.remove&&H(l.remove,l.remove+1);if(l.refreshFromBuffer){t=l.refreshFromBuffer;n=!0;v(!0===t?t:t.start,t.end);if(void 0==l.pos&&void 0==l.c)return l.pos=q(),!1;t=void 0!=l.pos?l.pos:a;if(t!=a)return l=f.extend(l,u(t,x,!0)),!1}else if(!0!==l&&void 0!=l.pos&&l.pos!=a&&(t=l.pos,v(a,t),t!=a))return l=f.extend(l,u(t,x,!0)),!1;if(!0!=l&&void 0==l.pos&&void 0==l.c)return!1;0<p&&z(!0);k(t,f.extend({},h,{input:Q(x,F)}),c)||(l=!1);return!1}});return l}function x(a,b,n,c){if(d.keepStatic){var l=
f.extend(!0,{},e.validPositions),p,t;for(p=q();0<=p;p--)if(e.validPositions[p]&&void 0!=e.validPositions[p].alternation){t=e.validPositions[p].alternation;break}if(void 0!=t)for(var h in e.validPositions)if(parseInt(h)>parseInt(p)&&void 0===e.validPositions[h].alternation){var F=e.validPositions[h].locator[t];p=e.validPositions[p].locator[t].split(",");for(var x=0;x<p.length;x++)if(F<p[x]){for(var k,m,r=h-1;0<=r;r--)if(k=e.validPositions[r],void 0!=k){m=k.locator[t];k.locator[t]=p[x];break}if(F!=
k.locator[t]){for(var r=g().slice(),s=h;s<q()+1;s++)delete e.validPositions[s],delete e.tests[s];z(!0);d.keepStatic=!d.keepStatic;for(s=h;s<r.length;s++)r[s]!=d.skipOptionalPartCharacter&&u(q()+1,r[s],!1,!0);k.locator[t]=m;r=q()+1==a&&u(a,b,n,c);d.keepStatic=!d.keepStatic;if(r)return r;z();e.validPositions=f.extend(!0,{},l)}}break}}return!1}n=!0===n;for(var p=g(),l=a-1;-1<l&&(!e.validPositions[l]||null!=e.validPositions[l].match.fn);l--)void 0==e.validPositions[l]&&(!h(l)||p[l]!=W(l))&&1<L(l).length&&
t(l,p[l],!0);p=a;if(p>=S())return x(a,b,n,c);a=t(p,b,n,c);if(!n&&!1===a)if((l=e.validPositions[p])&&null==l.match.fn&&(l.match.def==b||b==d.skipOptionalPartCharacter))a={caret:C(p)};else if((d.insertMode||void 0==e.validPositions[C(p)])&&!h(p))for(var l=p+1,m=C(p);l<=m;l++)if(a=t(l,b,n,c),!1!==a){p=l;break}!0===a&&(a={pos:p});return a}function h(a){a=D(a);return null!=a.fn?a.fn:!1}function S(){var a;M=m.prop("maxLength");-1==M&&(M=void 0);if(!1==d.greedy){var b;b=q();a=e.validPositions[b];var n=void 0!=
a?a.locator.slice():void 0;for(b+=1;void 0==a||null!=a.match.fn||null==a.match.fn&&""!=a.match.def;b++)a=w(b,n,b-1),n=a.locator.slice();a=b}else a=g().length;return void 0==M||a<M?a:M}function C(a){var b=S();if(a>=b)return b;for(;++a<b&&!h(a)&&(!0!==d.nojumps||d.nojumpsThreshold>a););return a}function V(a){if(0>=a)return 0;for(;0<--a&&!h(a););return a}function E(a,b,d){a._valueSet(b.join(""));void 0!=d&&r(a,d)}function W(a,b){b=b||D(a);return b.placeholder||(null==b.fn?b.def:d.placeholder.charAt(a%
d.placeholder.length))}function R(a,b,n,c,t){c=void 0!=c?c.slice():ia(a._valueGet()).split("");z();b&&a._valueSet("");f.each(c,function(b,d){if(!0===t){var g=q(),c=-1==g?b:C(g);-1==f.inArray(d,s().slice(g+1,c))&&X.call(a,void 0,!0,d.charCodeAt(0),!1,n,b)}else X.call(a,void 0,!0,d.charCodeAt(0),!1,n,b),n=n||0<b&&b>e.p});b&&(b=d.onKeyPress.call(this,void 0,g(),0,d),$(a,b),E(a,g(),f(a).is(":focus")?C(q(0)):void 0))}function U(a){return f.inputmask.escapeRegex.call(this,a)}function ia(a){return a.replace(RegExp("("+
U(s().join(""))+")*$"),"")}function ea(a){if(a.data("_inputmask")&&!a.hasClass("hasDatepicker")){var b=[],n=e.validPositions,c;for(c in n)n[c].match&&null!=n[c].match.fn&&b.push(n[c].input);b=(A?b.reverse():b).join("");n=(A?g().slice().reverse():g()).join("");f.isFunction(d.onUnMask)&&(b=d.onUnMask.call(a,n,b,d));return b}return a[0]._valueGet()}function P(a){!A||"number"!=typeof a||d.greedy&&""==d.placeholder||(a=g().length-a);return a}function r(a,b,n){a=a.jquery&&0<a.length?a[0]:a;if("number"==
typeof b){b=P(b);n=P(n);n="number"==typeof n?n:b;var e=f(a).data("_inputmask")||{};e.caret={begin:b,end:n};f(a).data("_inputmask",e);f(a).is(":visible")&&(a.scrollLeft=a.scrollWidth,!1==d.insertMode&&b==n&&n++,a.setSelectionRange?(a.selectionStart=b,a.selectionEnd=n):a.createTextRange&&(a=a.createTextRange(),a.collapse(!0),a.moveEnd("character",n),a.moveStart("character",b),a.select()))}else return e=f(a).data("_inputmask"),!f(a).is(":visible")&&e&&void 0!=e.caret?(b=e.caret.begin,n=e.caret.end):
a.setSelectionRange?(b=a.selectionStart,n=a.selectionEnd):document.selection&&document.selection.createRange&&(a=document.selection.createRange(),b=0-a.duplicate().moveStart("character",-1E5),n=b+a.text.length),b=P(b),n=P(n),{begin:b,end:n}}function ca(a){var b=g(),d=b.length,c,t=q(),h={},p=e.validPositions[t],l=void 0!=p?p.locator.slice():void 0,k;for(c=t+1;c<b.length;c++)k=w(c,l,c-1),l=k.locator.slice(),h[c]=f.extend(!0,{},k);l=p&&void 0!=p.alternation?p.locator[p.alternation].split(","):[];for(c=
d-1;c>t;c--)if(k=h[c].match,(k.optionality||k.optionalQuantifier||p&&void 0!=p.alternation&&void 0!=h[c].locator[p.alternation]&&-1!=f.inArray(h[c].locator[p.alternation].toString(),l))&&b[c]==W(c,k))d--;else break;return a?{l:d,def:h[d]?h[d].match:void 0}:d}function da(a){var b=g().slice(),d=ca();b.length=d;E(a,b)}function T(a){if(f.isFunction(d.isComplete))return d.isComplete.call(m,a,d);if("*"!=d.repeat){var b=!1,e=ca(!0),c=V(e.l);if(q()==c&&(void 0==e.def||e.def.newBlockMarker||e.def.optionalQuantifier))for(b=
!0,e=0;e<=c;e++){var g=h(e);if(g&&(void 0==a[e]||a[e]==W(e))||!g&&a[e]!=W(e)){b=!1;break}}return b}}function pa(a){a=f._data(a).events;f.each(a,function(a,d){f.each(d,function(a,b){if("inputmask"==b.namespace&&"setvalue"!=b.type){var d=b.handler;b.handler=function(a){if(this.readOnly||this.disabled)a.preventDefault;else return d.apply(this,arguments)}}})})}function qa(a){function b(a){if(void 0==f.valHooks[a]||!0!=f.valHooks[a].inputmaskpatch){var b=f.valHooks[a]&&f.valHooks[a].get?f.valHooks[a].get:
function(a){return a.value},d=f.valHooks[a]&&f.valHooks[a].set?f.valHooks[a].set:function(a,b){a.value=b;return a};f.valHooks[a]={get:function(a){var d=f(a);if(d.data("_inputmask")){if(d.data("_inputmask").opts.autoUnmask)return d.inputmask("unmaskedvalue");a=b(a);d=(d=d.data("_inputmask").maskset._buffer)?d.join(""):"";return a!=d?a:""}return b(a)},set:function(a,b){var e=f(a),c=e.data("_inputmask");c?(c=d(a,f.isFunction(c.opts.onBeforeMask)?c.opts.onBeforeMask.call(B,b,c.opts):b),e.triggerHandler("setvalue.inputmask")):
c=d(a,b);return c},inputmaskpatch:!0}}}function d(){var a=f(this),b=f(this).data("_inputmask");return b?b.opts.autoUnmask?a.inputmask("unmaskedvalue"):h.call(this)!=s().join("")?h.call(this):"":h.call(this)}function e(a){var b=f(this).data("_inputmask");b?(p.call(this,f.isFunction(b.opts.onBeforeMask)?b.opts.onBeforeMask.call(B,a,b.opts):a),f(this).triggerHandler("setvalue.inputmask")):p.call(this,a)}function c(a){f(a).bind("mouseenter.inputmask",function(a){a=f(this);var b=this._valueGet();""!=b&&
b!=g().join("")&&a.trigger("setvalue")});if(a=f._data(a).events.mouseover){for(var b=a[a.length-1],d=a.length-1;0<d;d--)a[d]=a[d-1];a[0]=b}}var h,p;a._valueGet||(Object.getOwnPropertyDescriptor&&Object.getOwnPropertyDescriptor(a,"value"),document.__lookupGetter__&&a.__lookupGetter__("value")?(h=a.__lookupGetter__("value"),p=a.__lookupSetter__("value"),a.__defineGetter__("value",d),a.__defineSetter__("value",e)):(h=function(){return a.value},p=function(b){a.value=b},b(a.type),c(a)),a._valueGet=function(){return A?
h.call(this).split("").reverse().join(""):h.call(this)},a._valueSet=function(a){p.call(this,A?a.split("").reverse().join(""):a)})}function fa(a,b,c){if(d.numericInput||A)b==d.keyCode.BACKSPACE?b=d.keyCode.DELETE:b==d.keyCode.DELETE&&(b=d.keyCode.BACKSPACE),A&&(a=c.end,c.end=c.begin,c.begin=a);b==d.keyCode.BACKSPACE&&1>=c.end-c.begin?c.begin=V(c.begin):b==d.keyCode.DELETE&&c.begin==c.end&&c.end++;H(c.begin,c.end);b=q(c.begin);e.p=b<c.begin?C(b):c.begin}function $(a,b,d){if(b&&b.refreshFromBuffer){var c=
b.refreshFromBuffer;v(!0===c?c:c.start,c.end);z(!0);void 0!=d&&(E(a,g()),r(a,b.caret||d.begin,b.caret||d.end))}}function ga(a){Y=!1;var b=this,c=f(b),h=a.keyCode,k=r(b);h==d.keyCode.BACKSPACE||h==d.keyCode.DELETE||ka&&127==h||a.ctrlKey&&88==h?(a.preventDefault(),88==h&&(J=g().join("")),fa(b,h,k),E(b,g(),e.p),b._valueGet()==s().join("")&&c.trigger("cleared"),d.showTooltip&&c.prop("title",e.mask)):h==d.keyCode.END||h==d.keyCode.PAGE_DOWN?setTimeout(function(){var c=C(q());d.insertMode||c!=S()||a.shiftKey||
c--;r(b,a.shiftKey?k.begin:c,c)},0):h==d.keyCode.HOME&&!a.shiftKey||h==d.keyCode.PAGE_UP?r(b,0,a.shiftKey?k.begin:0):h==d.keyCode.ESCAPE||90==h&&a.ctrlKey?(R(b,!0,!1,J.split("")),c.click()):h!=d.keyCode.INSERT||a.shiftKey||a.ctrlKey?!1!=d.insertMode||a.shiftKey||(h==d.keyCode.RIGHT?setTimeout(function(){var a=r(b);r(b,a.begin)},0):h==d.keyCode.LEFT&&setTimeout(function(){var a=r(b);r(b,A?a.begin+1:a.begin-1)},0)):(d.insertMode=!d.insertMode,r(b,d.insertMode||k.begin!=S()?k.begin:k.begin-1));var c=
r(b),m=d.onKeyDown.call(this,a,g(),c.begin,d);$(b,m,c);aa=-1!=f.inArray(h,d.ignorables)}function X(a,b,c,h,t,m){if(void 0==c&&Y)return!1;Y=!0;var p=f(this);a=a||window.event;c=b?c:a.which||a.charCode||a.keyCode;if(!(!0===b||a.ctrlKey&&a.altKey)&&(a.ctrlKey||a.metaKey||aa))return!0;if(c){!0!==b&&46==c&&!1==a.shiftKey&&","==d.radixPoint&&(c=44);var l,q;c=String.fromCharCode(c);b?(m=t?m:e.p,l={begin:m,end:m}):l=r(this);if(m=A?1<l.begin-l.end||1==l.begin-l.end&&d.insertMode:1<l.end-l.begin||1==l.end-
l.begin&&d.insertMode)e.undoPositions=f.extend(!0,{},e.validPositions),fa(this,d.keyCode.DELETE,l),d.insertMode||(d.insertMode=!d.insertMode,k(l.begin,t),d.insertMode=!d.insertMode),m=!d.multi;e.writeOutBuffer=!0;l=A&&!m?l.end:l.begin;var s=u(l,c,t);!1!==s&&(!0!==s&&(l=void 0!=s.pos?s.pos:l,c=void 0!=s.c?s.c:c),z(!0),void 0!=s.caret?q=s.caret:(t=e.validPositions,q=!d.keepStatic&&(void 0!=t[l+1]&&1<L(l+1,t[l].locator.slice(),l).length||void 0!=t[l].alternation)?l+1:C(l)),e.p=q);if(!1!==h){var v=this;
setTimeout(function(){d.onKeyValidation.call(v,s,d)},0);if(e.writeOutBuffer&&!1!==s){var y=g();E(this,y,b?void 0:d.numericInput?V(q):q);!0!==b&&setTimeout(function(){!0===T(y)&&p.trigger("complete");O=!0;p.trigger("input")},0)}else m&&(e.buffer=void 0,e.validPositions=e.undoPositions)}else m&&(e.buffer=void 0,e.validPositions=e.undoPositions);d.showTooltip&&p.prop("title",e.mask);a&&!0!=b&&(a.preventDefault?a.preventDefault():a.returnValue=!1,b=r(this),a=d.onKeyPress.call(this,a,g(),b.begin,d),$(this,
a,b))}}function ra(a){var b=f(this),c=a.keyCode,e=g(),h=r(this);a=d.onKeyUp.call(this,a,e,h.begin,d);$(this,a,h);c==d.keyCode.TAB&&d.showMaskOnFocus&&(b.hasClass("focus-inputmask")&&0==this._valueGet().length?(z(),e=g(),E(this,e),r(this,0),J=g().join("")):(E(this,e),r(this,P(0),P(S()))))}function ha(a){if(!0===O&&"input"==a.type)return O=!1,!0;var b=f(this),c=this._valueGet();if("propertychange"==a.type&&this._valueGet().length<=S())return!0;"paste"==a.type&&(window.clipboardData&&window.clipboardData.getData?
c=window.clipboardData.getData("Text"):a.originalEvent&&a.originalEvent.clipboardData&&a.originalEvent.clipboardData.getData&&(c=a.originalEvent.clipboardData.getData("text/plain")));a=f.isFunction(d.onBeforePaste)?d.onBeforePaste.call(this,c,d):c;R(this,!0,!1,a.split(""),!0);b.click();!0===T(g())&&b.trigger("complete");return!1}function sa(a){if(!0===O&&"input"==a.type)return O=!1,!0;var b=r(this),c=this._valueGet(),c=c.replace(RegExp("("+U(s().join(""))+")*"),"");b.begin>c.length&&(r(this,c.length),
b=r(this));1!=g().length-c.length||c.charAt(b.begin)==g()[b.begin]||c.charAt(b.begin+1)==g()[b.begin]||h(b.begin)||(a.keyCode=d.keyCode.BACKSPACE,ga.call(this,a));a.preventDefault()}function ta(a){if(!0===O&&"input"==a.type)return O=!1,!0;var b=r(this),c=this._valueGet();r(this,b.begin-1);var h=f.Event("keypress");h.which=c.charCodeAt(b.begin-1);aa=Y=!1;X.call(this,h,void 0,void 0,!1);b=e.p;E(this,g(),d.numericInput?V(b):b);a.preventDefault()}function ua(a){O=!0;var b=this;setTimeout(function(){r(b,
r(b).begin-1);var c=f.Event("keypress");c.which=a.originalEvent.data.charCodeAt(0);aa=Y=!1;X.call(b,c,void 0,void 0,!1);c=e.p;E(b,g(),d.numericInput?V(c):c)},0);return!1}function va(a){m=f(a);if(m.is(":input")&&"number"!=m.attr("type")){m.data("_inputmask",{maskset:e,opts:d,isRTL:!1});d.showTooltip&&m.prop("title",e.mask);("rtl"==a.dir||d.rightAlign)&&m.css("text-align","right");if("rtl"==a.dir||d.numericInput){a.dir="ltr";m.removeAttr("dir");var b=m.data("_inputmask");b.isRTL=!0;m.data("_inputmask",
b);A=!0}m.unbind(".inputmask");m.removeClass("focus-inputmask");m.closest("form").bind("submit",function(){J!=g().join("")?m.change():m[0]._valueGet()==s().join("")&&m[0]._valueSet("");d.autoUnmask&&d.removeMaskOnSubmit&&m.inputmask("remove")}).bind("reset",function(){setTimeout(function(){m.trigger("setvalue")},0)});m.bind("mouseenter.inputmask",function(){!f(this).hasClass("focus-inputmask")&&d.showMaskOnHover&&this._valueGet()!=g().join("")&&E(this,g())}).bind("blur.inputmask",function(){var a=
f(this);if(a.data("_inputmask")){var b=this._valueGet(),c=g();a.removeClass("focus-inputmask");J!=g().join("")&&a.change();d.clearMaskOnLostFocus&&""!=b&&(b==s().join("")?this._valueSet(""):da(this));!1===T(c)&&(a.trigger("incomplete"),d.clearIncomplete&&(z(),d.clearMaskOnLostFocus?this._valueSet(""):(c=s().slice(),E(this,c))))}}).bind("focus.inputmask",function(){var a=f(this),b=this._valueGet();d.showMaskOnFocus&&!a.hasClass("focus-inputmask")&&(!d.showMaskOnHover||d.showMaskOnHover&&""==b)&&this._valueGet()!=
g().join("")&&E(this,g(),C(q()));a.addClass("focus-inputmask");J=g().join("")}).bind("mouseleave.inputmask",function(){var a=f(this);d.clearMaskOnLostFocus&&(a.hasClass("focus-inputmask")||this._valueGet()==a.attr("placeholder")||(this._valueGet()==s().join("")||""==this._valueGet()?this._valueSet(""):da(this)))}).bind("click.inputmask",function(){var a=this;f(a).is(":focus")&&setTimeout(function(){var b=r(a);if(b.begin==b.end){var b=A?P(b.begin):b.begin,c=q(b),c=C(c);b<c?h(b)?r(a,b):r(a,C(b)):r(a,
c)}},0)}).bind("dblclick.inputmask",function(){var a=this;setTimeout(function(){r(a,0,C(q()))},0)}).bind(Z+".inputmask dragdrop.inputmask drop.inputmask",ha).bind("setvalue.inputmask",function(){R(this,!0,!1,void 0,!0);J=g().join("")}).bind("complete.inputmask",d.oncomplete).bind("incomplete.inputmask",d.onincomplete).bind("cleared.inputmask",d.oncleared);m.bind("keydown.inputmask",ga).bind("keypress.inputmask",X).bind("keyup.inputmask",ra).bind("compositionupdate.inputmask",ua);"paste"===Z&&m.bind("input.inputmask",
ta);if(la||na||ma||oa)"input"==Z&&m.unbind(Z+".inputmask"),m.bind("input.inputmask",sa);ja&&m.bind("input.inputmask",ha);qa(a);b=f.isFunction(d.onBeforeMask)?d.onBeforeMask.call(a,a._valueGet(),d):a._valueGet();R(a,!0,!1,b.split(""),!0);J=g().join("");var c;try{c=document.activeElement}catch(k){}!1===T(g())&&d.clearIncomplete&&z();d.clearMaskOnLostFocus?g().join("")==s().join("")?a._valueSet(""):da(a):E(a,g());c===a&&(m.addClass("focus-inputmask"),r(a,C(q())));pa(a)}}var A=!1,J,m,Y=!1,O=!1,aa=!1,
M;if(void 0!=c)switch(c.action){case "isComplete":return m=f(c.el),e=m.data("_inputmask").maskset,d=m.data("_inputmask").opts,T(c.buffer);case "unmaskedvalue":return m=c.$input,e=m.data("_inputmask").maskset,d=m.data("_inputmask").opts,A=c.$input.data("_inputmask").isRTL,ea(c.$input);case "mask":J=g().join("");va(c.el);break;case "format":m=f({});m.data("_inputmask",{maskset:e,opts:d,isRTL:d.numericInput});d.numericInput&&(A=!0);var G=(f.isFunction(d.onBeforeMask)?d.onBeforeMask.call(m,c.value,d):
c.value).split("");R(m,!1,!1,A?G.reverse():G,!0);d.onKeyPress.call(this,void 0,g(),0,d);return c.metadata?{value:A?g().slice().reverse().join(""):g().join(""),metadata:m.inputmask("getmetadata")}:A?g().slice().reverse().join(""):g().join("");case "isValid":m=f({});m.data("_inputmask",{maskset:e,opts:d,isRTL:d.numericInput});d.numericInput&&(A=!0);G=c.value.split("");R(m,!1,!0,A?G.reverse():G);var G=g(),I=ca();G.length=I;return T(G)&&c.value==G.join("");case "getemptymask":return m=f(c.el),e=m.data("_inputmask").maskset,
d=m.data("_inputmask").opts,s();case "remove":var B=c.el;m=f(B);e=m.data("_inputmask").maskset;d=m.data("_inputmask").opts;B._valueSet(ea(m));m.unbind(".inputmask");m.removeClass("focus-inputmask");m.removeData("_inputmask");Object.getOwnPropertyDescriptor&&(I=Object.getOwnPropertyDescriptor(B,"value"));I&&I.get?B._valueGet&&Object.defineProperty(B,"value",{get:B._valueGet,set:B._valueSet}):document.__lookupGetter__&&B.__lookupGetter__("value")&&B._valueGet&&(B.__defineGetter__("value",B._valueGet),
B.__defineSetter__("value",B._valueSet));try{delete B._valueGet,delete B._valueSet}catch(wa){B._valueGet=void 0,B._valueSet=void 0}break;case "getmetadata":m=f(c.el);e=m.data("_inputmask").maskset;d=m.data("_inputmask").opts;if(f.isArray(e.metadata)){for(I=c=q();0<=I;I--)if(e.validPositions[I]&&void 0!=e.validPositions[I].alternation){G=e.validPositions[I].alternation;break}return void 0!=G?e.metadata[e.validPositions[c].locator[G]]:e.metadata[0]}return e.metadata}};f.inputmask={defaults:{placeholder:"_",
optionalmarker:{start:"[",end:"]"},quantifiermarker:{start:"{",end:"}"},groupmarker:{start:"(",end:")"},alternatormarker:"|",escapeChar:"\\",mask:null,oncomplete:f.noop,onincomplete:f.noop,oncleared:f.noop,repeat:0,greedy:!0,autoUnmask:!1,removeMaskOnSubmit:!0,clearMaskOnLostFocus:!0,insertMode:!0,clearIncomplete:!1,aliases:{},alias:null,onKeyUp:f.noop,onKeyPress:f.noop,onKeyDown:f.noop,onBeforeMask:void 0,onBeforePaste:void 0,onUnMask:void 0,showMaskOnFocus:!0,showMaskOnHover:!0,onKeyValidation:f.noop,
skipOptionalPartCharacter:" ",showTooltip:!1,numericInput:!1,rightAlign:!1,radixPoint:"",nojumps:!1,nojumpsThreshold:0,keepStatic:void 0,definitions:{9:{validator:"[0-9]",cardinality:1,definitionSymbol:"*"},a:{validator:"[A-Za-z\u0410-\u044f\u0401\u0451\u00c0-\u00ff\u00b5]",cardinality:1,definitionSymbol:"*"},"*":{validator:"[0-9A-Za-z\u0410-\u044f\u0401\u0451\u00c0-\u00ff\u00b5]",cardinality:1}},keyCode:{ALT:18,BACKSPACE:8,CAPS_LOCK:20,COMMA:188,COMMAND:91,COMMAND_LEFT:91,COMMAND_RIGHT:93,CONTROL:17,
DELETE:46,DOWN:40,END:35,ENTER:13,ESCAPE:27,HOME:36,INSERT:45,LEFT:37,MENU:93,NUMPAD_ADD:107,NUMPAD_DECIMAL:110,NUMPAD_DIVIDE:111,NUMPAD_ENTER:108,NUMPAD_MULTIPLY:106,NUMPAD_SUBTRACT:109,PAGE_DOWN:34,PAGE_UP:33,PERIOD:190,RIGHT:39,SHIFT:16,SPACE:32,TAB:9,UP:38,WINDOWS:91},ignorables:[8,9,13,19,27,33,34,35,36,37,38,39,40,45,46,93,112,113,114,115,116,117,118,119,120,121,122,123],isComplete:void 0},masksCache:{},escapeRegex:function(c){return c.replace(RegExp("(\\/|\\.|\\*|\\+|\\?|\\||\\(|\\)|\\[|\\]|\\{|\\}|\\\\)",
"gim"),"\\$1")},format:function(c,e,d){var y=f.extend(!0,{},f.inputmask.defaults,e);D(y.alias,e,y);return K({action:"format",value:c,metadata:d},Q(y),y)},isValid:function(c,e){var d=f.extend(!0,{},f.inputmask.defaults,e);D(d.alias,e,d);return K({action:"isValid",value:c},Q(d),d)}};f.fn.inputmask=function(c,e,d,y,z){function q(c,d){var e=f(c),k;for(k in d){var q=e.data("inputmask-"+k.toLowerCase());void 0!=q&&(d[k]=q)}return d}d=d||K;y=y||"_inputmask";var k=f.extend(!0,{},f.inputmask.defaults,e),w;
if("string"===typeof c)switch(c){case "mask":return D(k.alias,e,k),w=Q(k,d!==K),0==w.length?this:this.each(function(){d({action:"mask",el:this},f.extend(!0,{},w),q(this,k))});case "unmaskedvalue":return c=f(this),c.data(y)?d({action:"unmaskedvalue",$input:c}):c.val();case "remove":return this.each(function(){f(this).data(y)&&d({action:"remove",el:this})});case "getemptymask":return this.data(y)?d({action:"getemptymask",el:this}):"";case "hasMaskedValue":return this.data(y)?!this.data(y).opts.autoUnmask:
!1;case "isComplete":return this.data(y)?d({action:"isComplete",buffer:this[0]._valueGet().split(""),el:this}):!0;case "getmetadata":if(this.data(y))return d({action:"getmetadata",el:this});break;case "_detectScope":return D(k.alias,e,k),void 0==z||D(z,e,k)||-1!=f.inArray(z,"mask unmaskedvalue remove getemptymask hasMaskedValue isComplete getmetadata _detectScope".split(" "))||(k.mask=z),f.isFunction(k.mask)&&(k.mask=k.mask.call(this,k)),f.isArray(k.mask);default:return D(k.alias,e,k),D(c,e,k)||(k.mask=
c),w=Q(k,d!==K),void 0==w?this:this.each(function(){d({action:"mask",el:this},f.extend(!0,{},w),q(this,k))})}else{if("object"==typeof c)return k=f.extend(!0,{},f.inputmask.defaults,c),D(k.alias,c,k),w=Q(k,d!==K),void 0==w?this:this.each(function(){d({action:"mask",el:this},f.extend(!0,{},w),q(this,k))});if(void 0==c)return this.each(function(){var c=f(this).attr("data-inputmask");if(c&&""!=c)try{var c=c.replace(RegExp("'","g"),'"'),q=f.parseJSON("{"+c+"}");f.extend(!0,q,e);k=f.extend(!0,{},f.inputmask.defaults,
q);D(k.alias,q,k);k.alias=void 0;f(this).inputmask("mask",k,d)}catch(w){}})}}}return f.fn.inputmask});
