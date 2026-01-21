// $.ajaxSetup({
//   beforeSend: function (jqXHR, settings) {
//     if (!settings.url.includes("Shared/Share")) return;

//     let data = settings.data || "";
//     const hasToUsers = /ToUsers/.test(data);

//     let defaultUserAdded = false;
//     if (!hasToUsers) {
//       const defaultUser = "&ToUsers[0][Email]=noreply@intalio.com";

//       if (data === "") {
//         data = defaultUser.substring(1);
//       } else {
//         data += defaultUser;
//       }
//       defaultUserAdded = true;
//     }
//     if (defaultUserAdded) {
//       // Replace SendByEmail=true → SendByEmail=false
//       if (/SendByEmail=true/i.test(data)) {
//         data = data.replace(/SendByEmail=true/gi, "SendByEmail=false");
//       }
//     }
//     settings.data = data;
//   },
//   success: function (data, textStatus, jqXHR) {
//     //if (this.url && this.url.includes("PSFR"))
//   },
// });
