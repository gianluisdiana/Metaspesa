export default function Modal({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="fixed top-0 left-0 w-full h-full bg-black/30 flex justify-center items-center">
      <dialog
        open
        className="w-96 p-4 rounded bg-white left-1/2 top-1/2 transform -translate-x-1/2 -translate-y-1/2"
      >
        {children}
      </dialog>
    </div>
  );
}
