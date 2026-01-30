import { POSTS_API_BASE } from "./config.js";

// GET all 
export async function getPosts() {
  const response = await fetch(`${POSTS_API_BASE}/posts`);

  if (!response.ok) 
    throw new Error("Ошибка при загрузке постов");

  return response.json();
}

// GET by id
export async function getPost(id) {
  const response = await fetch(`${POSTS_API_BASE}/posts/${id}`);

  if (!response.ok) 
    throw new Error(`Ошибка загрузки поста с id=${id}`);

  return response.json();
}

// CREATE 
export async function createPost(post) {
  const response = await fetch(`${POSTS_API_BASE}/posts`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(post)
  });

  if (!response.ok) 
    throw new Error("Ошибка при создании поста");

  if (response.status === 201 || response.status === 204) 
    return null;

  return response.json();
}

// UPDATE 
export async function updatePost(id, data) {
  const response = await fetch(`${POSTS_API_BASE}/posts/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data)
  });

  if (!response.ok) 
    throw new Error("Ошибка при обновлении поста");

  if (response.status === 204) 
    return null;

  return response.json();
}

// DELETE (SOFT)
export async function deletePost(id) {
  const response = await fetch(`${POSTS_API_BASE}/posts/${id}`, {
    method: "DELETE"
  });

  if (!response.ok) throw new Error("Ошибка при удалении поста");

  if (response.status === 204) 
    return null;

  return true;
}