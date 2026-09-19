import keycloak from "./keycloak";

let initialized = false;

export async function initAuth() {
  if (initialized) {
    return keycloak.authenticated;
  }

  const authenticated = await keycloak.init({
    onLoad: "check-sso",
    pkceMethod: "S256",
    checkLoginIframe: false
  });

  initialized = true;

  console.log("Keycloak authenticated:", keycloak.authenticated);
  console.log("Keycloak token:", keycloak.token);
  console.log("Keycloak token parsed:", keycloak.tokenParsed);
  console.log("Keycloak token parsed aud:", keycloak.tokenParsed?.aud);
  console.log("Keycloak token parsed scope:", keycloak.tokenParsed?.scope);
  console.log("Keycloak token parsed preferred_username:", keycloak.tokenParsed?.preferred_username);
  console.log("Keycloak token parsed email:", keycloak.tokenParsed?.email);

  return authenticated;
}

export function login() {
  return keycloak.login();
}

export function logout() {
  return keycloak.logout({
    redirectUri: window.location.origin
  });
}

export function isAuthenticated() {
  return keycloak.authenticated === true;
}

export function getToken() {
  return keycloak.token;
}

export function getUserId() {
  return keycloak.tokenParsed?.sub;
}

export function getUsername() {
  return (
    keycloak.tokenParsed?.preferred_username ??
    keycloak.tokenParsed?.name ??
    keycloak.tokenParsed?.email ??
    null
  );
}