import { USERS_API_BASE } from "./config.js";

// GET all
export async function getUsers() {
  const response = await fetch(`${USERS_API_BASE}/users`);

  if (!response.ok) 
    throw new Error("Ошибка при загрузке пользователей");

  return response.json();
}

// GET by id
export async function getUserById(id) {
  const response = await fetch(`${USERS_API_BASE}/users/${id}`);

  if (!response.ok) 
    throw new Error(`Ошибка при загрузке пользователя с Id '${id}'`);

  return response.json();
}

// CREATE
export async function createUser(user) {
  const response = await fetch(`${USERS_API_BASE}/users`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(user),
  });

  if (!response.ok) 
    throw new Error("Ошибка при создании пользователя");

  if (response.status === 201 || response.status === 204) 
    return null;

  return response.json();
}

// UPDATE
export async function updateUser(id, user) {
  const response = await fetch(`${USERS_API_BASE}/users/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(user),
  });

  if (!response.ok) 
    throw new Error("Ошибка при обновлении пользователя");

  if (response.status === 204) 
    return null;

  return response.json();
}

// DELETE (SOFT)
export async function deleteUser(id) {
  const response = await fetch(`${USERS_API_BASE}/users/${id}`, {
    method: "DELETE",
  });

  if (!response.ok) 
    throw new Error("Ошибка при удалении пользователя");
  
  if (response.status === 204) 
    return null;

  return true;
}