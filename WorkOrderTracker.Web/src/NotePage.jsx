import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

export default function NotePage() {
  const { id } = useParams();
  const [note, setNote] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadNote() {
      try {
        const resp = await fetch(`/api/workorders/notes/${id}`);
        if (!resp.ok) {
          throw new Error("HTTP GET request failed - note by ID");
        }
        const data = await resp.json();
        setNote(data);
      } catch (err) {
        setError(err.message);
      }
    }
    loadNote();
  }, [id]);

  if (error) return <p style={{ color: "red" }}>{error}</p>;
  if (!note) return <p>Loading...</p>;

  return (
    <div>
      <h1>Details for note #: {id}</h1>
      <br />
      <p id="Note-content">{note.content}</p>
    </div>
  );
}
