import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useNavigate } from "react-router-dom";

export default function WorkOrderPage() {
  const { id } = useParams();
  const [errors, setError] = useState("");
  const [notes, setNotes] = useState([]);
  const [count, setCount] = useState(0);
  const navigate = useNavigate();

  useEffect(() => {
    async function loadNotes() {
      try {
        const resp = await fetch(`/api/workorders/${id}/notes`);
        if (!resp.ok) {
          throw new Error("HTTP GET request failed - notes");
        }
        const data = await resp.json();
        setNotes(data.items);
        setCount(data.count);
      } catch (err) {
        setError(err.message);
      }
    }
    loadNotes();
  }, [id]);

  async function deleteWorkOrder(Id) {
    try {
      const resp = await fetch(`/api/workorders/${Id}`, {
        method: "DELETE",
      });
      if (!resp.ok) {
        throw new Error(`Failed to delete Work Order ${Id}`);
      }

      navigate(`/`);
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div>
      <h1>Details For Work Order: {id}</h1>
      <h2>Note count: {count}</h2>
      <br />

      <ol id="notes">
        Notes:
        {notes.map((m) => (
          <li key={m.id}>
            ID: {m.id}
            <br />
            <button onClick={() => navigate(`/note/${m.id}`)}>
              Get Details
            </button>
          </li>
        ))}
      </ol>
      <br />
      <button onClick={() => navigate(`/`)}>Back</button>
      <br />
      <button onClick={() => deleteWorkOrder(id)} disabled={!id}>
        Delete
      </button>
    </div>
  );
}
