using Microsoft.JSInterop;

namespace Registrar.Data; 

public class HtmlInteractor {
    private readonly IJSRuntime _jsRuntime;
    
    public HtmlInteractor(IJSRuntime jsRuntime) {
        _jsRuntime = jsRuntime;
    }

    public async Task CopyToClipboard(string text) {
        await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }

    public async Task<string> GetValue(string id) {
        return await _jsRuntime.InvokeAsync<string>("eval", "document.getElementById('" + id + "').value");
    }
    
    public async Task<string> GetHtml(string id) {
        return await _jsRuntime.InvokeAsync<string>("eval", "document.getElementById('" + id + "').innerHTML");
    }
    
    public async Task<string> GetText(string id) {
        return await _jsRuntime.InvokeAsync<string>("eval", "document.getElementById('" + id + "').innerText");
    }
    
    public async Task SetValue(string id, string value) {
        await _jsRuntime.InvokeVoidAsync("eval", "document.getElementById('" + id + "').value = '" + value + "'");
    }
    
    public async Task SetHtml(string id, string value) {
        await _jsRuntime.InvokeVoidAsync("eval", "document.getElementById('" + id + "').innerHTML = '" + value + "'");
    }
    
    public async Task SubmitForm(string id) {
        await _jsRuntime.InvokeVoidAsync("eval", "document.getElementById('" + id + "').submit()");
    }

    public async Task Confetti(bool runOnClick) {
        await _jsRuntime.InvokeVoidAsync("eval", $"confetti({runOnClick.ToString().ToLower()})");
    }
    
    public async Task InvokeCode(string code) {
        await _jsRuntime.InvokeVoidAsync("eval", code);
    }
    
    public async Task GetLanguage() {
        await _jsRuntime.InvokeVoidAsync("eval", "window.getCultureLang()");
    }
    
    public async Task SetLanguage(string lang) {
        await _jsRuntime.InvokeVoidAsync("eval", $"localStorage.setItem('lang','{lang.ToLower()}')");
    }
    
    public async Task RemoveAttribute(string id, string attribute) {
        await _jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{id}').removeAttribute('{attribute}')");
    }
    
    public async Task SetAttribute(string id, string attribute, string value) {
        await _jsRuntime.InvokeVoidAsync("eval", $"document.getElementById('{id}').setAttribute('{attribute}','{value}')");
    }

    public async Task MarkUnsavedChanges(bool mark) {
        if (mark) {
            await _jsRuntime.InvokeVoidAsync("eval", "window.unsavedChanges");
        }
        else {
            await _jsRuntime.InvokeVoidAsync("eval", "window.savedChanges");
        }
    }

    public async Task Log(string msg) {
        await _jsRuntime.InvokeVoidAsync("console.log", msg);
    }

}