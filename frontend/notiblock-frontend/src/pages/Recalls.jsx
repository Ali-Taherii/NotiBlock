import RecallForm from "../components/RecallForm";
import RecallList from "../components/RecallList";
import useRecalls from "../hooks/useRecalls";


export default function Recalls() {
  const { recalls, loading, error, refetch } = useRecalls();

  return (
    <div className="max-w-4xl mx-auto p-8">
      <h1 className="text-3xl font-bold mb-6">Recalls</h1>

      <div className="mb-10">
        <RecallForm refetch={refetch}/>
      </div>

      <h2 className="text-2xl font-semibold mb-4">Recall List</h2>

      {loading && <p>Loading recalls...</p>}
      {error && <p className="text-red-600">{error}</p>}
      {!loading && !error && <RecallList recalls={recalls} refetch={refetch} />}
    </div>
  );
}
