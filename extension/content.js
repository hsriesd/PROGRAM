chrome.storage.local.get("restrictionEnabled", (data) => {
  const restrictionEnabled = data.restrictionEnabled !== undefined ? data.restrictionEnabled : true;
  
  if (restrictionEnabled) {
    const allowedUrls = [
      "chrome://extensions/",
      "chrome://newtab",
      "https://school.littlefox.co.kr",
      "https://littlefox.co.kr"  // 추가된 부분
    ];

    const currentUrl = location.href;

    const isAllowed = allowedUrls.some(url => currentUrl.startsWith(url));

    if (!isAllowed) {
      alert("절루 가세요~!"); // 사용자에게 알림
      // background.js로 메시지 보내기
      chrome.runtime.sendMessage({ action: "openNewTab" });
    }
  }
});
chrome.commands.onCommand.addListener((command) => {
  if (command === "toggle-restriction") {
    restrictionEnabled = !restrictionEnabled;
    chrome.storage.local.set({ restrictionEnabled });
    
    // 토글 상태에 따른 메시지 전송
    chrome.runtime.sendMessage({ action: "showAlert", message: restrictionEnabled ? "ENABLED." : "DISABLED." });
  }
});
