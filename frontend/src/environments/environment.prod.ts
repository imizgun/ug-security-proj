export const environment = {
  production: true,
  apiUrl: 'http://localhost:8080',
  oauth: {
    issuer: 'http://localhost:8080',
    clientId: 'angular-spa',
    redirectUri: window.location.origin + '/callback',
    postLogoutRedirectUri: window.location.origin,
    scope: 'openid profile email roles',
    responseType: 'code',
    useSilentRefresh: false,
    pkce: true,
    showDebugInformation: false,
    strictDiscoveryDocumentValidation: false,
    skipIssuerCheck: true,
    clearHashAfterLogin: false,
    nonceStateSeparator: 'semicolon',
  }
};