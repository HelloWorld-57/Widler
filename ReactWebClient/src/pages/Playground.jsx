import { useEffect, useState } from "react";
import { getUsers, createUser, deleteUser } from "../api/usersApi";
import { getPosts, createPost, deletePost } from "../api/postsApi";

// тестовая страница для генерации данных 
// сделана быстро и криво для быстрой проверки, ни на что не претендует

export default function Playground() {
    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(false);

    async function loadData() {
    setLoading(true);

    try {
        const users = await getUsers();
        const posts = await getPosts();

        const result = users.map(user => ({
            user,
            curPosts: posts.filter(p => p.userId === user.id)
        }));

        setData(result);
    } catch (e) {
        console.error(e);
        alert("Ошибка загрузки данных");
    } finally {
        setLoading(false);
    }
}

  useEffect(() => {
    loadData();
  }, []);

    async function generateData() {
        
        setLoading(true);

        try {
            for (let i = 0; i < 5; i++) {
                await createUser({
                    name: `User${i}`,
                    secondName: `Test${i}`,
                    email: `user${i}@test.com`,
                    birthDate: new Date(1990, 0, 1)
                });
        }

        const users = await getUsers();

        for (const user of users) {
            for (let j = 0; j < 3; j++) {
                await createPost({
                    caption: `Post ${j} of ${user.id}`,
                    content: "Lorem ipsum",
                    userId: user.id
                });
            }
        }

        await loadData(); 
        } catch (e) {
        console.error(e);
        alert("Ошибка генерации данных");
        } finally {
        setLoading(false);
        }
    }

  
  async function removeUser(userId) {
    if (!confirm("Удалить пользователя?")) return;
    await deleteUser(userId);
    await loadData();
  }

  async function removePost(userId, postId) {
    await deletePost(postId);
    await loadData();
  }

  return (
    <div style={{ padding: 20 }}>
      <h1>Playground</h1>

      <div style={{ marginBottom: 10 }}>
        <button onClick={generateData} disabled={loading}>
          {loading ? "Генерация..." : "Сгенерировать данные"}
        </button>

        <button
          onClick={loadData}
          disabled={loading}
          style={{ marginLeft: 10 }}
        >
          Обновить
        </button>
      </div>

      <hr />

      {data.map(({ user, curPosts }) => (
        <div key={user.id} style={{ marginBottom: 20 }}>
          <strong>
            {user.fullName} ({user.email})
          </strong>
          <button
            style={{ marginLeft: 10 }}
            onClick={() => removeUser(user.id)}
          >
            Удалить
          </button>

          <ul>
            {curPosts.map(p => (
              <li key={p.id}>
                {p.caption}
                <button
                  style={{ marginLeft: 10 }}
                  onClick={() => removePost(user.id, p.id)}
                >
                  Удалить
                </button>
              </li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  );
}
