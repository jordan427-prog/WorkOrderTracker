import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useParams } from "react-router-dom";

export default function CreateNotePage() {
  const [content, setContent] = useState("");
  const [error, setError] = useState([]);
  const navigate = useNavigate();
  const { id } = useParams();

  async function submitHandle(e) {
    e.preventDefault();
    try {
      const resp = await fetch(`/api/workorders/${id}/notes`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          Content: content,
        }),
      });

      if (!resp.ok) {
        const msg = await resp.text();
        throw new Error(msg);
      }
      navigate(`/notes/${id}`);
    } catch (err) {
      setError([err.message]);
    }
  }

  return (
    <div>
      <h1>Create a New Note</h1>
      {error.length > 0 && <p>{error[0]}</p>}
      <form onSubmit={submitHandle}>
        <br />
        <textarea
          placeholder="Note Content"
          value={content}
          onChange={(e) => setContent(e.target.value)}
        />
        <button type="submit">Create</button>
      </form>
    </div>
  );
}
