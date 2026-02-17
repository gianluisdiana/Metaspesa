import Modal from './modal';

export default function BooleanModalForm({
  children,
  confirm,
  close,
}: Readonly<{
  children: React.ReactNode;
  close: () => void;
  confirm: () => void;
}>) {
  return (
    <Modal>
      <form method="dialog">
        {children}
        <ModalFormMenu close={close} confirm={confirm} />
      </form>
    </Modal>
  );
}

const ModalFormMenu = ({
  close,
  confirm,
}: Readonly<{
  close: () => void;
  confirm: () => void;
}>) => {
  return (
    <menu className="flex justify-end gap-4 mr-5">
      <button type="button" className="cursor-pointer" onClick={close}>
        Cancelar
      </button>
      <button type="submit" className="cursor-pointer" onClick={confirm}>
        Confirmar
      </button>
    </menu>
  );
};
