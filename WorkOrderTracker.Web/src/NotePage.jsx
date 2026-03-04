import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useNavigate } from "react-router-dom";

export default function NotePage() {
  const { id } = useParams();
  const [note, setNote] = useState(null);
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const [parentId, setParentId] = useState(0);
  useEffect(() => {
    async function loadNote() {
      try {
        const resp = await fetch(`/api/workorders/notes/${id}`);
        if (!resp.ok) {
          throw new Error("HTTP GET request failed - note by ID");
        }
        const data = await resp.json();
        setNote(data);
        setParentId(data.workOrderId);
      } catch (err) {
        setError(err.message);
      }
    }
    loadNote();
  }, [id]);

  if (error) return <p style={{ color: "red" }}>{error}</p>;
  if (!note) return <p>Loading...</p>;

  async function deleteNote(noteId) {
    try {
      const resp = await fetch(`/api/workorders/${parentId}/notes/${noteId}`, {
        method: "DELETE",
      });
      if (!resp.ok) {
        throw new Error(`Failed to delete note ${noteId}`);
      }

      navigate(`/notes/${parentId}`);
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div>
      <h1>Details for note #: {id}</h1>
      <br />
      <p id="Note-content">{note.content}</p>
      <br />
      <button onClick={() => navigate(`/notes/${parentId}`)}>Back</button>
      <br />
      <button onClick={() => deleteNote(id)} disabled={!note}>
        Delete
      </button>
    </div>
  );
}
