let restrictionEnabled = true;


chrome.commands.onCommand.addListener((command) => {
  if (command === "toggle-restriction") {
    restrictionEnabled = !restrictionEnabled;
    chrome.storage.local.set({ restrictionEnabled });
    
    // 토글 상태에 따른 알림 표시
    const notificationOptions = {
      type: "basic",
      title: "Restriction Toggle",
      message: restrictionEnabled ? "ENABLED." : "DISABLED."
    };

    chrome.notifications.create(notificationOptions);
  }
});


// 초기 상태를 로드
chrome.storage.local.get("restrictionEnabled", (data) => {
  restrictionEnabled = data.restrictionEnabled !== undefined ? data.restrictionEnabled : true;
});


// 초기 상태를 로드
chrome.storage.local.get("restrictionEnabled", (data) => {
  restrictionEnabled = data.restrictionEnabled !== undefined ? data.restrictionEnabled : true;
});
chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.action === "openNewTab") {
    chrome.tabs.create({ url: "chrome://newtab" }, (tab) => {
      // 새로운 탭을 열고 기존 탭을 닫는 코드(선택적)
      chrome.tabs.remove(sender.tab.id);  // 기존 탭을 닫음
    });
  }
});
