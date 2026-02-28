import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

export default function WorkOrdersListPage() {
  const [workorders, setOrders] = useState([]);
  const [total, setTotal] = useState(0);
  const [error, setError] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    async function loadOrders() {
      try {
        const resp = await fetch("/api/workorders");
        if (!resp.ok) {
          throw new Error("Failed GET request - WorkOrders");
        }
        const data = await resp.json();
        setOrders(data.items);
        setTotal(data.total);
      } catch (err) {
        setError(err.message);
      }
    }

    loadOrders();
  }, []);

  return (
    <div>
      <h1>Work Orders Tracker</h1>
      <h2>Total orders:</h2>
      <h3>{total}</h3>
      <ol id="orders">
        {workorders.map((o) => (
          <li key={o.id}>
            Item: {o.title}
            <br />
            Status: {o.status}
            <br />
            Description: {o.description}
            <br />
            <button onClick={() => navigate(`/notes/${o.id}`)}>
              Get Details
            </button>
            <br />
          </li>
        ))}
      </ol>
    </div>
  );
}
