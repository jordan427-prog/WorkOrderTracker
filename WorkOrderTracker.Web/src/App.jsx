import "./App.css";

import { useEffect, useState } from "react";

export default function App() {
  // this is where we will store orders
  const [workorders, setOrders] = useState([]);
  const [total, setTotal] = useState(0);

  useEffect(() => {
    async function loadOrders() {
      const resp = await fetch("/api/workorders");
      const data = await resp.json();
      setOrders(data.items);
      setTotal(data.total);
    }

    loadOrders();
  }, []);

  return (
    <div>
      <h1>Work Orders Tracker</h1>
      <h2>Total orders:</h2>
      <h3>{total}</h3>
      <ul id="orders">
        {workorders.map((o) => (
          <li key={o.id}>
            {o.title}
            <br />
            Status: {o.status}
            <br />
            Description: {o.description}
          </li>
        ))}
      </ul>
    </div>
  );
}
