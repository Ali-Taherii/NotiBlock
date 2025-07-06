import { Link } from "react-router-dom";

export default function Home() {
  return (
    <div className="p-8">
      <h1 className="text-3xl mb-4 font-bold">Welcome to NotiBlock</h1>
      <Link to="/recalls" className="text-red-600 underline">
        Go to Recall Form
      </Link>
    </div>
  );
}
