import "./App.css";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import WorkOrdersListPage from "./WorkOrdersListPage";
import WorkOrderPage from "./WorkOrderPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<WorkOrdersListPage />} />
        <Route path="/notes/:id" element={<WorkOrderPage />} />
      </Routes>
    </BrowserRouter>
  );
}
