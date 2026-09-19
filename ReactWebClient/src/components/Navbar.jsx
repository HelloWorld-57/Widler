import React from "react";
import { Link } from "react-router-dom";
import { login, logout, isAuthenticated, getUsername } from "../auth/auth";

export default function Navbar() {
  const authenticated = isAuthenticated();

  return (
    <nav className="navbar">
      <Link to="/">Главная</Link>
      <Link to="/playground">Playground</Link>
      {authenticated ? (
        <>
          <span>
            {getUsername()}
          </span>

          <button onClick={logout}>
            Logout
          </button>
        </>
      ) : (
        <button onClick={login}>
          Login
        </button>
      )}
    </nav>
  );
}