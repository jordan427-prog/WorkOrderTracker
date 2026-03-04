import { useState } from "react";
import { useNavigate } from "react-router-dom";

export default function CreateWorkOrderPage() {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState([]);
  const navigate = useNavigate();

  async function submitHandle(e) {
    e.preventDefault();
    try {
      const resp = await fetch("/api/workorders", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          Title: title,
          Description: description,
        }),
      });

      if (!resp.ok) {
        const msg = await resp.text();
        throw new Error(msg);
      }
      navigate("/");
    } catch (err) {
      setError([err.message]);
    }
  }

  return (
    <div>
      <h1>Create a New Work Order</h1>
      {error.length > 0 && <p>{error[0]}</p>}
      <form onSubmit={submitHandle}>
        <br />
        <input
          type="text"
          placeholder="Title"
          value={title}
          required
          onChange={(e) => setTitle(e.target.value)}
        />
        <br />
        <textarea
          placeholder="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
        <button type="submit">Create</button>
      </form>
    </div>
  );
}
