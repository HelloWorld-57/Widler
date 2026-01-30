import React from "react";
import { Link } from "react-router-dom";

export default function NotFound() {
  return (
    <div style={{ textAlign: "center", marginTop: "100px" }}>
      <h1>404</h1>
      <p>Такой страницы не существует</p>
      <Link to="/" style={{ color: "#007bff", textDecoration: "none" }}>
        Вернуться на главную
      </Link>
    </div>
  );
}