type TextFieldProps = {
  id: string;
  label: string;
  icon: string;
  type?: string;
  placeholder: string;
};

export default function TextField({
  id,
  label,
  icon,
  type = 'text',
  placeholder,
}: Readonly<TextFieldProps>) {
  return (
    <div className="space-y-stack-sm">
      <label
        className="font-label-md text-label-md text-on-surface block"
        htmlFor={id}
      >
        {label}
      </label>
      <div className="relative">
        <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant">
          {icon}
        </span>
        <input
          className="w-full bg-surface-container pl-12 pr-4 py-3 rounded-lg border-none focus:ring-2 focus:ring-tertiary-container focus:bg-surface-container-lowest font-body-md text-body-md text-on-surface placeholder:text-on-surface-variant/50 transition-colors"
          id={id}
          name={id}
          placeholder={placeholder}
          type={type}
        />
      </div>
    </div>
  );
}
